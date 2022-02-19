using System;
using System.Collections.Generic;

namespace Mercury.View;

/// <summary>
/// This class contains appointment group methods
/// </summary>
internal static class AppointmentGroupHelper
{
    /// <summary>
    /// Gets the group count
    /// </summary>
    /// <param name="list">List to get group from</param>
    /// <param name="beginIndex">Index to start from</param>
    /// <returns>The group count</returns>
    public static int GetGroupCount(IList<AppointmentItem> list, int beginIndex)
    {
        var (begin, length) = list[beginIndex].GetFractionOfDay();
        var end = begin + length;
        var count = 1;
        for (var i = beginIndex + 1; i < list.Count; i++)
        {
            var (b, l) = list[i].GetFractionOfDay();
            var e = b + l;
            if (e > end)
                end = e;
            if (list[i].Indent == 0 && b > e)
                break;

            count++;
        }
        return count;
    }

    /// <summary>
    /// Get the indentation count within a group
    /// </summary>
    /// <param name="list">List to search in</param>
    /// <param name="beginIndex">Begin index of group</param>
    /// <param name="count">Count of group</param>
    /// <param name="ident">Indentation to get count for</param>
    /// <returns>List with items matching the identation</returns>
    public static IList<AppointmentItem> GetIdentationItems(IList<AppointmentItem> list, int beginIndex, int count, int ident)
    {
        var result = new List<AppointmentItem>();
        for (var i = beginIndex; i < beginIndex + count; i++)
        {
            if (list[i].Indent == ident)
                result.Add(list[i]);
        }
        return result;
    }

    /// <summary>
    /// Gets the end of the group
    /// </summary>
    /// <param name="list">List to search in</param>
    /// <param name="beginIndex">Begin index of group</param>
    /// <param name="count">Count of group</param>
    /// <returns>The end as a fraction of the day</returns>
    public static double GetEnd(IList<AppointmentItem> list, int beginIndex, int count)
    {
        var (begin, length) = list[beginIndex].GetFractionOfDay();
        var end = begin + length;

        for (var i = beginIndex + 1; i < beginIndex + count; i++)
        {
            var (b, l) = list[i].GetFractionOfDay();
            var e = b + l;
            if (e > end)
                end = e;
        }

        return end;
    }
    /// <summary>
    /// Gets the indentation count for a group
    /// </summary>
    /// <param name="list">List to search in</param>
    /// <param name="beginIndex">Begin index of group</param>
    /// <param name="count">Count of group</param>
    /// <returns>Indentation count</returns>
    public static int GetIdentationCount(IList<AppointmentItem> list, int beginIndex, int count)
    {
        var result = 0;
        for (var i = beginIndex; i < beginIndex + count; i++)
        {
            if (list[i].Indent > result)
                result = list[i].Indent;
        }

        return result + 1;
    }
}