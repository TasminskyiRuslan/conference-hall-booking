using ConferenceHallBooking.Application.Common.Models;
using ConferenceHallBooking.Application.Configuration;
using ConferenceHallBooking.Application.Interfaces.Bookings;
using ConferenceHallBooking.Domain.Entities;
using ConferenceHallBooking.Domain.Exceptions;
using ConferenceHallBooking.Domain.Interfaces;
using Microsoft.Extensions.Options;

namespace ConferenceHallBooking.Application.Services.Bookings;

/// <summary>
/// Splits the booking range at pricing-rule boundaries and applies
/// each rule's multiplier to its segment. No matching rule в†’ multiplier 1.0.
/// </summary>
public class PricingService(
    IPricingRuleRepository pricingRuleRepository,
    IOptions<PricingSettings> pricingSettings) : IPricingService
{
    private readonly TimeZoneInfo _timeZone =
        TimeZoneInfo.FindSystemTimeZoneById(pricingSettings.Value.TimeZoneId);
    /// <summary>Computes the hall cost and adds option prices for the time slot.</summary>
    public async Task<PricingResult> CalculatePriceAsync(
        decimal baseHourlyRate,
        IReadOnlyCollection<decimal>? optionPrices,
        DateTimeOffset startTime,
        DateTimeOffset endTime,
        CancellationToken cancellationToken = default)
    {
        if (endTime <= startTime)
        {
            throw new InvalidBookingTimeException(startTime, endTime);
        }

        if (baseHourlyRate <= 0)
        {
            throw new InvalidBaseHourlyRateException(baseHourlyRate);
        }

        var rules = await pricingRuleRepository.GetOrderedAsync(cancellationToken);
        return Calculate(baseHourlyRate, optionPrices, startTime, endTime, rules);
    }

    private PricingResult Calculate(
        decimal baseHourlyRate,
        IReadOnlyCollection<decimal>? optionPrices,
        DateTimeOffset startTime,
        DateTimeOffset endTime,
        IReadOnlyList<PricingRule> rules)
    {
        var boundaryPoints = GetBoundaryPoints(startTime, endTime, rules);
        var hallCost = CalculateHallCost(baseHourlyRate, boundaryPoints, rules);
        var optionsCost = optionPrices?.Sum() ?? 0m;

        var roundedHallCost = Math.Round(hallCost, 2, MidpointRounding.AwayFromZero);
        var roundedOptionsCost = Math.Round(optionsCost, 2, MidpointRounding.AwayFromZero);
        var roundedTotalCost = roundedHallCost + roundedOptionsCost;

        return new PricingResult(roundedHallCost, roundedOptionsCost, roundedTotalCost);
    }

    private decimal CalculateHallCost(
        decimal baseHourlyRate,
        IReadOnlyList<DateTimeOffset> boundaryPoints,
        IReadOnlyList<PricingRule> rules)
    {
        decimal hallCost = 0m;

        for (var i = 0; i < boundaryPoints.Count - 1; i++)
        {
            var segmentStart = boundaryPoints[i];
            var segmentEnd = boundaryPoints[i + 1];

            var hours = (decimal)(segmentEnd - segmentStart).TotalHours;
            var multiplier = GetMultiplierForSegment(segmentStart, segmentEnd, rules);

            hallCost += baseHourlyRate * hours * multiplier;
        }

        return hallCost;
    }

    private decimal GetMultiplierForSegment(
        DateTimeOffset segmentStart,
        DateTimeOffset segmentEnd,
        IReadOnlyList<PricingRule> rules)
    {
        var middle = segmentStart.AddTicks((segmentEnd - segmentStart).Ticks / 2);
        var localPoint = TimeZoneInfo.ConvertTime(middle, _timeZone);
        var time = TimeOnly.FromDateTime(localPoint.DateTime);

        return GetMultiplierForTime(time, rules);
    }

    private List<DateTimeOffset> GetBoundaryPoints(
        DateTimeOffset start,
        DateTimeOffset end,
        IReadOnlyList<PricingRule> rules)
    {
        var points = new HashSet<DateTimeOffset> { start, end };

        if (rules.Count == 0)
        {
            return [.. points.OrderBy(point => point)];
        }

        var startDate = DateOnly.FromDateTime(start.Date);
        var endDate = DateOnly.FromDateTime(end.Date);

        for (var date = startDate; date <= endDate; date = date.AddDays(1))
        {
            foreach (var rule in rules)
            {
                AddBoundaryIfInRange(points, start, end, date, rule.StartTime);
                AddBoundaryIfInRange(points, start, end, date, rule.EndTime);
            }
        }

        return [.. points.OrderBy(point => point)];
    }

    private void AddBoundaryIfInRange(
        HashSet<DateTimeOffset> points,
        DateTimeOffset rangeStart,
        DateTimeOffset rangeEnd,
        DateOnly date,
        TimeOnly time)
    {
        var wallTime = date.ToDateTime(time);
        if (_timeZone.IsInvalidTime(wallTime))
        {
            return;
        }

        var point = new DateTimeOffset(
            TimeZoneInfo.ConvertTimeToUtc(wallTime, _timeZone),
            TimeSpan.Zero);

        if (point > rangeStart && point < rangeEnd)
        {
            points.Add(point);
        }
    }

    private static decimal GetMultiplierForTime(TimeOnly time, IReadOnlyList<PricingRule> rules)
    {
        var rule = rules
            .FirstOrDefault(r => time >= r.StartTime && time < r.EndTime);

        return rule?.Multiplier ?? 1.0m;
    }
}
