using UnityEngine;
using UnityEditor;
using System.IO;

public class GerenciadorGrade : MonoBehaviour {
    public int largura;
    public int altura;
    public float tamanhoUnidade = 1f;
    [Range(0f, 1f)] public float chanceNaoAndavel = 0.2f;
    public GameObject tileBranco;
    public GameObject tilePreto;
    public GameObject tileInicio;
    public GameObject tileDestino;

    private Node[,] grade;
    public Vector2 posicaoInicio { get; private set; } // Get publico, mas set privado
    public Vector2 posicaoDestino { get; private set; } // Get publico, mas set privado
    private Transform pastaTiles;

    private void Start() {
        GerarNovaGrade();

        GameObject.Find("Resolvedor").GetComponent<Resolvedor>().ResetarPosicaoResolvedor();
        // A linha acima acha o resolvedor e o coloca na posicao de partida
    }
    public void GerarNovaGrade() {
        Transform pastaExistente = transform.Find("TilesGerados");

        if (pastaExistente != null) {
            DestroyImmediate(pastaExistente.gameObject);
        }

        pastaTiles = new GameObject("TilesGerados").transform;
        pastaTiles.SetParent(transform);

        GerarPontosFixos();
        GerarGrade();
    }

    void GerarPontosFixos() {
        bool inverter = Random.value > 0.5f;
        int inicioColuna = inverter ? largura - 2 : 0;
        int destinoColuna = inverter ? 0 : largura - 2;

        posicaoInicio = new Vector2(inicioColuna + Random.Range(0, 2), Random.Range(0, altura));
        posicaoDestino = new Vector2(destinoColuna + Random.Range(0, 2), Random.Range(0, altura));
    }

    void GerarGrade() {
        grade = new Node[largura, altura];

        for (int x = 0; x < largura; x++) {
            for (int y = 0; y < altura; y++) {
                Vector2 posicaoAtual = new Vector2(x, y);
                bool ehAndavel = Random.value > chanceNaoAndavel || posicaoAtual == posicaoInicio || posicaoAtual == posicaoDestino;

                GameObject tilePrefab = ehAndavel ? tileBranco : tilePreto;
                if (posicaoAtual == posicaoInicio) tilePrefab = tileInicio;
                if (posicaoAtual == posicaoDestino) tilePrefab = tileDestino;

                GameObject tile = Instantiate(tilePrefab, transform.position + new Vector3(x * tamanhoUnidade, y * tamanhoUnidade, 0), Quaternion.identity, pastaTiles);
                Node node = tile.GetComponent<Node>();
                node.posicao = posicaoAtual;
                node.ehAndavel = ehAndavel;

                grade[x, y] = node;

                tile.name = $"Tile {x},{y}";
            }
        }
    }

    public Node[,] GetGrade() { return grade; }
}
