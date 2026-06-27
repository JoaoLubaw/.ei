using Pontuei.Api.Dtos.Objects;

namespace Pontuei.Api.Commons;

/// <summary>
/// Messages corresponding to InternalResultCode values, used for user-facing error responses.
/// </summary>
public static class ErrorMessages
{
    private static readonly Dictionary<InternalResultCode, string> _messages = new()
    {
        // Authentication
        [InternalResultCode.INVALID_CREDENTIALS] = "E-mail ou senha inválidos.",
        [InternalResultCode.UNLOGGED] = "Você precisa estar autenticado para acessar este recurso.",
        [InternalResultCode.REFRESH_TOKEN_EXPIRED] = "Sua sessão expirou. Por favor, faça login novamente.",
        [InternalResultCode.NOT_ADMIN] = "Você não tem permissão de administrador para realizar esta ação.",
        [InternalResultCode.NOT_ALLOWED_TO_EDIT_USER] = "Você não tem permissão para editar este usuário.",
        [InternalResultCode.NOT_ALLOWED_TO_CREATE_OR_EDIT_ITEM] = "Você não tem permissão para criar ou editar este item.",
        [InternalResultCode.NOT_ALLOWED_TO_GET_THIS_ITEM] = "Você não tem permissão para visualizar este item.",
        [InternalResultCode.NOT_ALLOWED_TO_GET_THIS_USER] = "Você não tem permissão para visualizar este usuário.",
        [InternalResultCode.INVALID_RESET_PASSWORD_TOKEN] = "O token de redefinição de senha é inválido ou expirou.",
        [InternalResultCode.TOO_MANY_RESET_REQUESTS] = "Muitas tentativas de redefinição de senha. Aguarde alguns minutos e tente novamente.",
        [InternalResultCode.TOO_MANY_INVALID_TOKEN_ATTEMPTS] = "Muitas tentativas inválidas. O código foi bloqueado por segurança.",
        [InternalResultCode.TOO_MANY_REQUESTS] = "Muitas requisições em pouco tempo. Por favor, aguarde antes de tentar novamente.",
        [InternalResultCode.EMAIL_NOT_VERIFIED] = "Seu e-mail ainda não foi verificado. Verifique sua caixa de entrada.",
        [InternalResultCode.EMAIL_ALREADY_VERIFIED] = "Este e-mail já foi verificado anteriormente.",

        // Validation
        [InternalResultCode.INVALID_PARAMETERS] = "Os parâmetros informados são inválidos.",
        [InternalResultCode.MISSING_INFORMATION] = "Informações obrigatórias não foram fornecidas.",
        [InternalResultCode.INFO_ALREADY_EXISTS] = "Este registro já existe no sistema.",
        [InternalResultCode.INVALID_TYPE] = "O tipo informado é inválido.",
        [InternalResultCode.INVALID_USER_EMAIL] = "O endereço de e-mail informado é inválido.",
        [InternalResultCode.INVALID_CURRENT_PASSWORD] = "A senha atual informada está incorreta.",
        [InternalResultCode.INVALID_USER_PASSWORD] = "A senha não atende aos requisitos mínimos de segurança.",
        [InternalResultCode.INVALID_INPUT] = "Os dados enviados são inválidos. Verifique e tente novamente.",
        [InternalResultCode.EMAIL_ALREADY_TAKEN] = "Este e-mail já está em uso por outra conta.",
        [InternalResultCode.PASSWORDS_DO_NOT_MATCH] = "As senhas informadas não coincidem.",
        [InternalResultCode.INVALID_USER_PHONE_NUMBER] = "O número de telefone informado é inválido.",
        [InternalResultCode.INVALID_CODE] = "O código informado é inválido ou expirou.",
        [InternalResultCode.TRANSACTION_NOT_PENDING] = "Esta transação não está mais pendente e não pode ser alterada.",

        // Enntity
        [InternalResultCode.ENTITY_NOT_FOUND] = "O recurso solicitado não foi encontrado.",

        // System
        [InternalResultCode.UNHANDLED_ERROR] = "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.",
        [InternalResultCode.RUNTIME_ERROR] = "Erro interno durante o processamento da requisição.",
        [InternalResultCode.DATABASE_CONNECTION] = "Falha ao conectar ao banco de dados. Tente novamente mais tarde.",
        [InternalResultCode.TIMEOUT] = "A requisição demorou mais do que o esperado. Tente novamente.",
        [InternalResultCode.METHOD_NOT_ALLOWED] = "Método não permitido para este recurso.",
        [InternalResultCode.SERVICE_UNAVAILABLE] = "O serviço está temporariamente indisponível. Tente novamente em breve.",
        [InternalResultCode.EMAIL_SEND_ERROR] = "Não foi possível enviar o e-mail. Verifique o endereço e tente novamente.",
    };

    /// <summary>
    /// Returns the error message corresponding to the given InternalResultCode.
    /// If the code is not mapped, returns a generic error message.
    /// </summary>
    public static string Get(InternalResultCode code) =>
        _messages.TryGetValue(code, out string? message)
            ? message
            : "Ocorreu um erro inesperado.";
}