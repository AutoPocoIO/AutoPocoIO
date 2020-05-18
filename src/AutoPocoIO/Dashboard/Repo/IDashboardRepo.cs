using System;

namespace AutoPocoIO.Dashboard.Repos
{
    internal interface IDashboardRepo
    {
        int FailedRequests(int daysAgo);
        int FailedRequestsTime(int daysAgo);
        Tuple<string[], int[], int[]> HourlyRequest();
        Tuple<string[], int[], int[]> WeeklyRequest();
        int SuccessFulRequests(int daysAgo);
        int SuccessFulRequestsTime(int daysAgo);
        int TotalRequests(int daysAgo);
        int TotalRequestsTime(int daysAgo);
        int UnauthorizedRequest(int daysAgo);
        int UnauthorizedRequestTime(int daysAgo);
    }
}