using Microsoft.EntityFrameworkCore;
using PRLab.Domain.Model.Entity;

namespace PRLab.Infrastructure.DB.Query;

public static class UserQueryExtensions
{
    public static IQueryable<User> WithFullAggregate(
        this IQueryable<User> query)
    {
        return query;
    }

    public static IQueryable<User> ActiveOnly(
        this IQueryable<User> query)
    {
        return query
            .Where(user => !user.Audit.IsDeleted);
    }

    public static IQueryable<User> ForFullRead(
        this IQueryable<User> query)
    {
        return query
            .WithFullAggregate()
            .ActiveOnly()
            .AsNoTracking();
    }

    public static IQueryable<User> ForFullWrite(
        this IQueryable<User> query)
    {
        return query
            .WithFullAggregate()
            .ActiveOnly();
    }
}