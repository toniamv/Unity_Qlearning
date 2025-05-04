using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class Qlearning
{
    private GerenciadorGrade gerenciador;
    private float[,] tabelaQ;
    private int gridWidth;
    private int gridHeight;
    
    //definindo parametros do Qlearning
    private float taxaDeAprendizado = 0.2f;
    private float fatorDeDesconto = 0.9f;
    private float taxaDeExploracao = 0.2f;
    private int numEpisodios = 1000;
    
    //vetor de direcoes possiveis considerando 4 lados
    private Vector2Int[] direcoesPossiveis = {
        new Vector2Int(0, 1), //cima
        new Vector2Int(1, 0), //direita
        new Vector2Int(0, -1),//baixo
        new Vector2Int(-1, 0),//esquerda
    };

    public Qlearning(GerenciadorGrade gerenciador)
    {
        this.gerenciador = gerenciador;
        gridWidth = gerenciador.largura;
        gridHeight = gerenciador.altura;
        IniciaTabelaQ();
    }

   public void printaTabelaQ()
    {
        //construindo string da tabela Q para apresentar itens
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("\n=== Matriz Q Formatada ===");
        sb.AppendLine("Formato: [Estado (x,y)] → Ações [Cima, Direita, Baixo, Esquerda]");
        sb.AppendLine("----------------------------");

        for (int y = gridHeight - 1; y >= 0; y--) //imprime o eixo y da matriz de cima pra baixo
        {
            for (int x = 0; x < gridWidth; x++)
            {
                int estadoAtual = calculaEstadoPorPosicao(new Vector2Int(x, y));
                sb.Append($"[{x},{y}]:\t");

                for (int acao = 0; acao < direcoesPossiveis.Length; acao++)
                {
                    sb.Append($"{tabelaQ[estadoAtual, acao].ToString("F6")}");
                    if (acao < direcoesPossiveis.Length - 1) sb.Append(" | ");
                }
                sb.AppendLine();
            }
        }

        Debug.Log(sb.ToString());
    }

    private void IniciaTabelaQ()
    {
        int qntEstados = gridWidth * gridHeight;
        int qtdAcoes = direcoesPossiveis.Length;
        tabelaQ = new float[qntEstados, qtdAcoes];
        for(int i = 0; i < qntEstados; i++)
            for(int j = 0; j < qtdAcoes; j++)
                //O codigo estava encontrando dificuldade em prosseguir a partir de 0, entao 
                // adicionamos um valor inicial aleatorio para ajudar a encontrar a solucao
                tabelaQ[i,j] = Random.Range(-0.01f, 0.01f);
    }

    public List<Node> encontraCaminho(Vector2 posinicio, Vector2 posdestino)
    {
        printaTabelaQ();
        treinaModelo(posinicio, posdestino);
        return caminhoOtimoTQ(posinicio, posdestino);
    }

    private void treinaModelo(Vector2 posinicio, Vector2 posdestino)
    {
        for (int ep = 0; ep < numEpisodios; ep++)
        {
            Vector2Int posAtual = Vector2Int.RoundToInt(posinicio);
            bool encontrouMeta = false;
            int passos = 0;
            int maxpassos = gridWidth * gridHeight * 2;

            //enquanto nao encontra meta e nao passa da quantidade maxima de passos
            //o episodio continua
            while (!encontrouMeta && passos < maxpassos)
            {
                passos++;
                int estadoAtual = calculaEstadoPorPosicao(posAtual);
                //escolher nova acao a partir do estado atual
                int acao = escolheAcao(estadoAtual, taxaDeExploracao);
                Vector2Int proxPosicao = posAtual + direcoesPossiveis[acao];

                //se a proxima posicao nao for dentro do grid, pune estado atual
                if (!dentroDoGrid(proxPosicao))
                {
                    tabelaQ[estadoAtual, acao] = -10f;
                    continue;
                }

                //define recompensa
                float recompensa = calculaRecompensa(proxPosicao, posdestino);

                //atualizar valor Q
                int nextState = calculaEstadoPorPosicao(proxPosicao);
                float maxNextQ = getMaxValorQ(nextState);
                tabelaQ[estadoAtual, acao] += taxaDeAprendizado * 
                    (recompensa + fatorDeDesconto * maxNextQ - tabelaQ[estadoAtual, acao]);

                //teste de meta
                if (proxPosicao == Vector2Int.RoundToInt(posdestino))
                {
                    encontrouMeta = true;
                }

                posAtual = proxPosicao;
            }
        }
    }

    private List<Node> caminhoOtimoTQ(Vector2 posinicio, Vector2 posdestino)
    {
        List<Node> caminho = new List<Node>();
        Node[,] grid = gerenciador.GetGrade();
        
        Vector2Int posAtual = Vector2Int.RoundToInt(posinicio);
        Vector2Int meta = Vector2Int.RoundToInt(posdestino);
        int maxpassos = gridWidth * gridHeight * 2;
        int passos = 0;

        caminho.Add(grid[posAtual.x, posAtual.y]);

        while (posAtual != meta && passos < maxpassos)
        {
            passos++;
            int estadoAtual = calculaEstadoPorPosicao(posAtual);
            int bestAcao = getBestAcao(estadoAtual);
            Vector2Int proxPosicao = posAtual + direcoesPossiveis[bestAcao];

            if (!dentroDoGrid(proxPosicao))
            {
                break;
            }

            caminho.Add(grid[proxPosicao.x, proxPosicao.y]);
            posAtual = proxPosicao;
        }

        return caminho;
    }

    private int escolheAcao(int estadoAtual, float epsilon)
    {
        if (Random.value < epsilon)
        {
            //componente de exploracao
            return Random.Range(0, direcoesPossiveis.Length);
        }
        else
        {
            //componente de explo
            return getBestAcao(estadoAtual);
        }
    }

    private int getBestAcao(int estadoAtual)
    {
        int bestAcao = 0;
        float maxQ = float.MinValue;

        for (int i = 0; i < direcoesPossiveis.Length; i++)
        {
            if (tabelaQ[estadoAtual, i] > maxQ)
            {
                maxQ = tabelaQ[estadoAtual, i];
                bestAcao = i;
            }
        }

        return bestAcao;
    }

    private float getMaxValorQ(int estadoAtual)
    {
        float maxQ = float.MinValue;
        for (int i = 0; i < direcoesPossiveis.Length; i++)
        {
            if (tabelaQ[estadoAtual, i] > maxQ)
            {
                maxQ = tabelaQ[estadoAtual, i];
            }
        }
        return maxQ;
    }

    private float calculaRecompensa(Vector2Int position, Vector2 posdestino)
    {
        Node[,] grid = gerenciador.GetGrade();
        
        // Verificar se é posição final
        if (position == Vector2Int.RoundToInt(posdestino))
        {
            return 1f;
        }

        // Verificar se é obstáculo
        if (!grid[position.x, position.y].ehAndavel)
        {
            return -1f;
        }

        return 0;
    }

    private bool dentroDoGrid(Vector2Int position)
    {
        //Se a posicao alvo esta dentro do grid
        return position.x >= 0 && position.x < gridWidth && 
               position.y >= 0 && position.y < gridHeight;
    }

    private int calculaEstadoPorPosicao(Vector2Int position)
    {
        return position.y * gridWidth + position.x;
    }
}