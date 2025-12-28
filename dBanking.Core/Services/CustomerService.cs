using dBanking.Core.Entities;
using dBanking.Core.Repository_Contracts;
using dBanking.Core.ServiceContracts;

namespace dBanking.Core.Services
{
    /// <summary>
    /// Application service for managing customer-related operations.
    /// </summary>  
    public sealed class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _customers;
        private readonly IPublishEndpoint _bus;

        // TODO: Inject IAuditRepository (when implemented), IIdempotencyStore (e.g., Redis) if desired
        public CustomerService(ICustomerRepository customers, IPublishEndpoint bus)
        {
            _customers = customers;
            _bus = bus;
        }

        public async Task<Customer> CreateAsync(Customer input, string? idempotencyKey = null, CancellationToken ct = default)
        {
            // Optional: honor idempotency (hook point)
            // if (!string.IsNullOrEmpty(idempotencyKey))
            // {
            //     var existsForKey = await _idempotency.ExistsAsync(idempotencyKey, ct);
            //     if (existsForKey) return await _idempotency.GetResultAsync<Customer>(idempotencyKey, ct);
            // }

            // Basic dedupe by email/phone
            var duplicate = await _customers.ExistsByEmailOrPhoneAsync(input.Email, input.Phone, ct);
            if (duplicate)
                throw new InvalidOperationException("Duplicate customer detected. Email or phone already exists.");

            // Persist
            await _customers.AddAsync(input, ct);
            await _customers.SaveChangesAsync(ct);

            // Publish domain event
            await _bus.Publish(new CustomerCreated(input.CustomerId, input.Email, input.Phone), ct);

            // Optional: store idempotency result
            // if (!string.IsNullOrEmpty(idempotencyKey))
            //     await _idempotency.StoreResultAsync(idempotencyKey, input, ct);

            return input;
        }

        public Task<Customer?> GetAsync(Guid customerId, CancellationToken ct = default)
            => _customers.GetByIdAsync(customerId, ct);

        public Task<Customer?> GetByEmailOrPhoneAsync(string? email, string? phone, CancellationToken ct = default)
            => _customers.GetByEmailOrPhoneAsync(email, phone, ct);

        public async Task<Customer> UpdateAsync(Customer customer, CancellationToken ct = default)
        {
            // Ensure the entity exists before updating (optional safeguard)
            var existing = await _customers.GetByIdAsync(customer.CustomerId, ct);
            if (existing is null)
                throw new KeyNotFoundException($"Customer '{customer.CustomerId}' not found.");

            // Example: only allow certain fields to change here; others via dedicated flows (contacts/address/etc.)
            existing.FirstName = customer.FirstName;
            existing.LastName = customer.LastName;
            existing.Dob = customer.Dob;
            existing.UpdatedAt = DateTime.UtcNow;

            await _customers.UpdateAsync(existing, ct);
            await _customers.SaveChangesAsync(ct);

            // Optional: publish an update event (define it when needed)
            // await _bus.Publish(new CustomerUpdated(existing.CustomerId, existing.FirstName, existing.LastName), ct);

            return existing;
        }
    }

}
