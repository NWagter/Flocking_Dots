using UnityEngine;
using System;

public class ContentSO : ScriptableObject
{
    private string m_guidString;
    private Guid m_guid;

    public Guid GetId()
    {
        return m_guid;
    }

    private void OnValidate()
    {
        if(m_guidString == string.Empty)
        {
            m_guid = Guid.NewGuid();
            m_guidString = m_guid.ToString();
        }
    }

    public virtual void Init()
    {

    }

}