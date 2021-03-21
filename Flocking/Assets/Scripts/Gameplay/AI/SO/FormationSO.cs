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
    [SerializeField]
    private UnitSO _unit;

    public string formationName => _formationName;
    public int formationWidth => _formationWidth;
    public int formationHeight => _formationHeight;
    public UnitSO unit => _unit;
}