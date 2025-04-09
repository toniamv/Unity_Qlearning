using UnityEngine;
using System;

public class Node : MonoBehaviour, IComparable<Node> {
    public Vector2 posicao { get; set; }
    public bool ehAndavel { get; set; } = true;
    public float custoG { get; set; }
    public float custoH { get; set; } // Heuristica
    public float custoF => custoG + custoH; // Advindo da formula do A*
    public Node pai { get; set; } // A refer�ncia para o noh anterior
    public int CompareTo(Node outro)
    {
        if (outro == null) return -1;
        //Comparando custoF
        if (this.custoF <= outro.custoF)
            return -1;
        else
            return 1;
    }

    public override bool Equals(object obj)
    {
        if (obj == null || !(obj is Node)) return false;

        Node outro = (Node)obj;
        return this.posicao == outro.posicao;
    }

    public override int GetHashCode()
    {
        return posicao.GetHashCode();
    }

}
