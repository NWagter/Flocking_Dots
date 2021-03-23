using System;
using System.Collections.Generic;
using UnityEngine;

public class ContentLibrary<T> : MonoBehaviour where T : ContentSO
{
    #region Singleton
    public static ContentLibrary<T> GetInstance()
    {
        if (ms_instance == null)
        {
            ms_instance = FindObjectOfType<ContentLibrary<T>>();
        }
        return ms_instance;
    }

    private static ContentLibrary<T> ms_instance;
    #endregion

    [SerializeField] protected T[] m_activeContent;

    private Dictionary<Guid, T> m_content = new Dictionary<Guid, T>();

#if UNITY_EDITOR
    public void SetActiveContent(T[] activeContent)
    {
        this.m_activeContent = activeContent;
    }
#endif

    public T[] GetAllContent() => m_activeContent;

    public T GetContent(Guid a_id)
    {
        Debug.Assert(m_content.ContainsKey(a_id), "Building does not exists!");

        return m_content[a_id];
    }

    public Type GetContent<Type>(Guid a_id) where Type : ContentSO
    {
        Debug.Assert(m_content.ContainsKey(a_id), "Content does not exists!");

        ContentSO building = m_content[a_id];

        Debug.Assert(building is T, "Content is not of correct type!");

        return (Type)building;
    }

    private void Start()
    {
        for (int i = 0; i < m_activeContent.Length; i++)
        {
            T content = m_activeContent[i];

            content.Init();
            this.m_content.Add(content.GetId(), content);
        }
    }

    private void OnDestroy()
    {
        Destroy(ms_instance);
        ms_instance = null;
    }
}
