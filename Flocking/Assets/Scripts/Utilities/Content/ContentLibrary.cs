using System;
using System.Collections.Generic;
using UnityEngine;

public class ContentLibrary<T> : MonoBehaviour where T : ContentSO
{
    #region Singleton
    public static ContentLibrary<T> GetInstance()
    {
        if (instance == null)
        {
            instance = FindObjectOfType<ContentLibrary<T>>();
        }
        return instance;
    }

    private static ContentLibrary<T> instance;
    #endregion

    [SerializeField] protected T[] activeContent;

    private Dictionary<Guid, T> content = new Dictionary<Guid, T>();

#if UNITY_EDITOR
    public void SetActiveContent(T[] activeContent)
    {
        this.activeContent = activeContent;
    }
#endif

    public T[] GetAllContent() => activeContent;

    public T GetContent(Guid id)
    {
        Debug.Assert(content.ContainsKey(id), "Building does not exists!");

        return content[id];
    }

    public Type GetContent<Type>(Guid id) where Type : ContentSO
    {
        Debug.Assert(content.ContainsKey(id), "Content does not exists!");

        ContentSO building = content[id];

        Debug.Assert(building is T, "Content is not of correct type!");

        return (Type)building;
    }

    private void Start()
    {
        for (int i = 0; i < activeContent.Length; i++)
        {
            T content = activeContent[i];

            content.Init();
            this.content.Add(content.GetId(), content);
        }
    }

    private void OnDestroy()
    {
        Destroy(instance);
        instance = null;
    }
}
