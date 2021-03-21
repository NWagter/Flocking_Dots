using UnityEngine;
using System;

public class ContentSO : ScriptableObject
{
    private string guid;
    private Guid _guid;

    public Guid GetId()
    {
        return _guid;
    }

    private void OnValidate()
    {
        if(guid == string.Empty)
        {
            _guid = Guid.NewGuid();
            guid = _guid.ToString();
        }
    }

    public virtual void Init()
    {

    }

}