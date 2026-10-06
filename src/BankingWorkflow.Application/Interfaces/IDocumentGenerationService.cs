namespace BankingWorkflow.Application.Interfaces;

public interface IDocumentGenerationService
{
    Task<byte[]> GenerateLoanAgreementDocxAsync(Guid applicationId, CancellationToken cancellationToken = default);
    Task<byte[]> GenerateDecisionCertificateDocxAsync(Guid applicationId, CancellationToken cancellationToken = default);
}
