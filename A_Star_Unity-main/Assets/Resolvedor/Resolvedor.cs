using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework.Interfaces;
using Unity.VisualScripting;
using UnityEngine;

public class Resolvedor : MonoBehaviour {
    public GerenciadorGrade gerenciador;
    public Color corAndado;
    public float velocidade = 2f;

    private Coroutine moverPorCaminhoCoroutine;


    public void ResolverLabirinto() {

        Node[,] grade = gerenciador.GetGrade(); // Pega a grade do problema

        List<Node> caminho = AEstrela(gerenciador.posicaoInicio, gerenciador.posicaoDestino);
        

        moverPorCaminhoCoroutine = StartCoroutine(MoverPorCaminhoCoroutine(caminho));
    }

    private List<Node> AEstrela(Vector2 posicaoInicio, Vector2 posicaoDestino) {

        Node[,] grade = gerenciador.GetGrade();
        
        List<Node> resultado = new List<Node>();
        List<Node> aberto = new List<Node>();
        HashSet<Node> fechado = new HashSet<Node>();

        Node nodeInicial = grade[(int)posicaoInicio.x, (int)posicaoInicio.y];
        Node nodeFinal = grade[(int)posicaoDestino.x, (int)posicaoDestino.y];

        nodeInicial.custoG = 0;
        nodeInicial.custoH = Vector2.Distance(nodeInicial.posicao, nodeFinal.posicao);
        nodeInicial.pai = null;

        aberto.Add(nodeInicial);

        while(aberto.Count > 0){
            //Ordena lista de elementos a serem explorados
            Node p = aberto[0];
            aberto.RemoveAt(0);
            //Testa primeiro elemento da lista para a meta
            if(p.posicao.Equals(nodeFinal.posicao)){
                resultado.Insert(0, p);
                Node no = p.pai;
                //Se for a meta, cria lista de caminhos através dos nós pai
                while(no.pai != null){
                    resultado.Insert(0, no);
                    no = no.pai;
                }
                return resultado;
            }
            //Caso nao seja meta, avalia seus vizinhos
            else{
                //Para cada vizinho, se não tiver sido avaliado ainda e se é um 
                // caminho andavel, calcula custos, define pai como o no sendo 
                // explorado e insere na lista aberta
                foreach(Node n in GetVizinhos(p, grade)){
                    if(!fechado.Contains(n) && n.ehAndavel){
                        float novoCustoG = p.custoG+1;

                        if(aberto.Contains(n)){
                            int index = aberto.IndexOf(n);
                            if(index >= 0){
                                if(aberto[index].custoG >= novoCustoG){
                                    aberto[index].custoG = novoCustoG;
                                    aberto[index].pai = p;
                                }
                            }
                        }else{
                            n.custoG = novoCustoG;
                            n.custoH = Vector2.Distance(n.posicao, nodeFinal.posicao);
                            n.pai = p;
                            aberto.Add(n);
                        }
                    }
                }
                fechado.Add(p);
                // Debug.Log("Antes de ordenar");
                // StringBuilder strAberto = new StringBuilder("[");
                // foreach(Node n in aberto){
                //     strAberto.Append("(" + n.posicao[0] + "," + n.posicao[1] + "-" + n.custoF + ")");
                // }
                // strAberto.Append("]");
                // Debug.Log(strAberto);

                aberto.Sort();

                // Debug.Log("Apos");
                // strAberto = new StringBuilder("[");
                // foreach(Node n in aberto){
                //     strAberto.Append("(" + n.posicao[0] + "," + n.posicao[1] + "-" + n.custoF + ")");
                // }
                // strAberto.Append("]");
                // Debug.Log(strAberto);
            }
        }
        
        return null; // Caso nao encontre caminho
    }
    

    private List<Node> GetVizinhos(Node node, Node[,] grade) { // Funcao auxiliar para achar os vizinhos
        List<Node> vizinhos = new List<Node>();
        int x = (int)node.posicao.x;
        int y = (int)node.posicao.y;

        // Checa os vizinhos nas 4 direcoes
        if (x > 0) vizinhos.Add(grade[x - 1, y]); // Esquerda
        if (x < grade.GetLength(0) - 1) vizinhos.Add(grade[x + 1, y]); // Direita
        if (y > 0) vizinhos.Add(grade[x, y - 1]); // Baixo
        if (y < grade.GetLength(1) - 1) vizinhos.Add(grade[x, y + 1]); // Cima
        
        // // Checa os vizinhos nas 8 direcoes
        // if (x > 0) vizinhos.Add(grade[x - 1, y]); // Esquerda
        // if (x > 0 && y > 0) vizinhos.Add(grade[x - 1, y-1]); // Diagonal Inferior Esquerda
        // if (x > 0 && y < grade.GetLength(1) - 1) vizinhos.Add(grade[x - 1, y + 1]); // Diagonal Superior Esquerda

        // if (x < grade.GetLength(0) - 1) vizinhos.Add(grade[x + 1, y]); // Direita
        // if (x < grade.GetLength(0) - 1 && y > 0) vizinhos.Add(grade[x + 1, y - 1]); // Diagonal Inferior Direita
        // if (x < grade.GetLength(0) - 1 && y < grade.GetLength(1) - 1) vizinhos.Add(grade[x + 1, y + 1]); // Diagonal Superior Esquerda
        // if (y > 0) vizinhos.Add(grade[x, y - 1]); // Baixo
        // if (y < grade.GetLength(1) - 1) vizinhos.Add(grade[x, y + 1]); // Cima

        return vizinhos;
    }

    private void ColorirTile(Color cor, GameObject tile) {
        tile.GetComponent<SpriteRenderer>().color = cor;
    }

    public void ResetarPosicaoResolvedor() {
        StopAllCoroutines();
        moverPorCaminhoCoroutine = null;
        transform.position = (Vector3)gerenciador.posicaoInicio + gerenciador.transform.position;
    }

    private IEnumerator MoverPorCaminhoCoroutine(List<Node> caminho) {
        if(moverPorCaminhoCoroutine != null) yield break; // Quebrando pois o jogador ja esta andando

        if (caminho == null || caminho.Count == 0) { // Quebrando pois o caminho nao existe, ou eh impossivel
            Debug.LogWarning("Caminho Impossivel");
            yield break;
        } 

        for (int i = 0; i < caminho.Count; i++) {
            Node nodoAtual = caminho[i];

            ColorirTile(corAndado, nodoAtual.gameObject);

            if (i < caminho.Count - 1) {
                Node proximoNodo = caminho[i + 1];
                Vector2 posicaoAtual = nodoAtual.transform.position; // Pegando a posicao relativa ao mundo
                Vector2 posicaoDestino = proximoNodo.transform.position; // Pegando a posicao relativa ao mundo

                yield return StartCoroutine(MoverParaPosicaoCoroutine(posicaoAtual, posicaoDestino));
            }
        }
        moverPorCaminhoCoroutine = null;
    }

    private IEnumerator MoverParaPosicaoCoroutine(Vector3 posicaoAtual, Vector3 posicaoDestino) {
        float tempo = 0f;
        float distancia = Vector3.Distance(posicaoAtual, posicaoDestino);

        while (tempo < distancia / velocidade) {
            tempo += Time.deltaTime;
            float interpolacao = tempo / (distancia / velocidade);
            transform.position = Vector3.Lerp(posicaoAtual, posicaoDestino, interpolacao);
            yield return null;
        }

        transform.position = posicaoDestino;
    }
}
