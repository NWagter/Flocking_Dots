using System;
using UnityEngine;


[CreateAssetMenu(fileName = "Formation", menuName = "Formations/FormationObject", order = 1)]
public class FormationSO : ContentSO
{
    [SerializeField]
    private string _formationName;
    [SerializeField]
    private int _formationWidth = 5;
    [SerializeField]
    private int _formationHeight = 5;

    public float _cohesionBias = 0.2f;

    public float _seperationBias = 1.0f;

    public float _alignmentBias = 0.8f;

    public float _boundsBias = 0.6f;

    public float _desiredBias = 1.0f;

    [SerializeField]
    private UnitSO _unit;

    public string formationName => _formationName;
    public int formationWidth => _formationWidth;
    public int formationHeight => _formationHeight;

    public float cohesionBias => _cohesionBias;

    public float seperationBias => _seperationBias;

    public float alignmentBias => _alignmentBias;

    public float boundsBias => _boundsBias;

    public float desiredBias => _desiredBias;

    public UnitSO unit => _unit;
}