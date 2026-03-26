using System.Net.Http.Json;
using sos100_bibliotek.Models;

namespace sos100_bibliotek.Services;

public class ReservationService
{
    private readonly HttpClient _httpClient;

    public ReservationService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<ReservationViewModel>> GetReservationsAsync()
    {
        var reservations = await _httpClient.GetFromJsonAsync<List<ReservationViewModel>>("api/reservations");
        return reservations ?? new List<ReservationViewModel>();
    }

    public async Task<bool> CreateReservationAsync(ReservationViewModel reservation)
    {
        var response = await _httpClient.PostAsJsonAsync("api/reservations", reservation);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteReservationAsync(int id)
    {
        var response = await _httpClient.DeleteAsync($"api/reservations/{id}");
        return response.IsSuccessStatusCode;
    }
}