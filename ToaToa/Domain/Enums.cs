namespace ToaToa.Domain;

public enum FormaPagamento
{
    Dinheiro = 0,
    Cartao = 1,
    Pix = 2
}

public enum StatusCaixa
{
    Aberto = 0,
    Fechado = 1
}

public enum TipoMovimentoCaixa
{
    Sangria = 0,
    Suprimento = 1
}
