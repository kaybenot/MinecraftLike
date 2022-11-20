using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Biome : MonoBehaviour
{
    public BlockLayer StartLayer;
    public List<BlockLayer> AdditionalLayers;
    public bool UseDomainWarping = true;
    public DomainWarping DomainWarping;
}
