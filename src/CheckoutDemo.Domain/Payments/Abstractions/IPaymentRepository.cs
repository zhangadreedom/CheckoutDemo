using CheckoutDemo.Domain.Payments.Entities;

namespace CheckoutDemo.Domain.Payments.Abstractions
{
    public interface IPaymentRepository
    {
        /// <summary>
        /// 根据主键 Id 获取 Payment。
        /// 用于大部分内部查询（例如 GetPaymentById API）。
        /// </summary>
        Task<Payment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// 根据业务 Reference 获取 Payment。
        /// 适合从订单号 / 外部系统 reference 反查 Payment。
        /// </summary>
        Task<Payment?> GetByReferenceAsync(string reference, CancellationToken cancellationToken = default);

        /// <summary>
        /// 根据 Checkout 返回的 payment id（例如 webhook 里带的 id）获取本地 Payment。
        /// 用于 ProcessWebhook 时关联本地记录。
        /// </summary>
        Task<Payment?> GetByCheckoutPaymentIdAsync(
            string checkoutPaymentId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 根据 payment session id（ps_...）获取 Payment。
        /// 如果你想在前端只拿 session id 查询状态，可以使用。
        /// </summary>
        Task<Payment?> GetByPaymentSessionIdAsync(
            string paymentSessionId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 新建 Payment。
        /// 一般在创建 Payment Session 时调用。
        /// </summary>
        Task AddAsync(Payment payment, CancellationToken cancellationToken = default);

        /// <summary>
        /// 更新已有 Payment。
        /// 在 EF 实现里通常只是标记实体为 Modified（或啥都不干，依赖跟踪）。
        /// 在 In-Memory 实现里可能需要显式替换。
        /// </summary>
        void Update(Payment payment);
    }
}
