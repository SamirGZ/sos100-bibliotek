using ReservationApi.Models;

namespace ReservationApi.Services;

public interface IReservationUpstreamClient
{
    Task<UserApiProfile?> GetUserAsync(int userId, CancellationToken cancellationToken = default);

    Task<CatalogueBook?> GetBookAsync(int bookId, CancellationToken cancellationToken = default);
}
