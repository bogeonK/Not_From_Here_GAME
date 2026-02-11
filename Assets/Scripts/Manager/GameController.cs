using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class GameController : MonoBehaviour
{

    [Header("UI References")]
    [SerializeField] private DialogueUI dialogueUI;
    public DialogueUI DialogueUI => dialogueUI;

    [Header("AI References")]
    [SerializeField] private OpenAIManager openAI;
    public OpenAIManager OpenAI => openAI;

    void Awake()
    {
        instance = this;

        ConnectBaseScriptableObject();

        Register<EndingSystemManager, EndingSystemConfigSO>(config => new EndingSystemManager(config));
        Register<NpcChoiceManager, NpcChoiceConfigSO>(config => new NpcChoiceManager(config)
);

        GetControllerAll();
        InitAll();
        ActiveOffAll();
       
    }

    void OnDisable()
    {
        foreach (var manager in managerMap.Values)
        {
            manager.Destory();
        }
    }



    void ConnectBaseScriptableObject()
    {
        dicBaseScriptableObject.Clear();

        foreach (var so in baseScriptableObjects)
        {
            if (so == null) continue;
            dicBaseScriptableObject[so.GetType()] = so;
        }
    }


    private void Register<TManager, TConfig>(Func<TConfig, TManager> factory) where TManager : baseManager where TConfig : BaseScriptableObject
    {
    
        TConfig config = (TConfig)dicBaseScriptableObject[typeof(TConfig)];
        TManager manager = factory(config);
        RegisterMap(manager);
    }

    private void RegisterMap<T1>(T1 manager) where T1 : baseManager
    {
        managerMap[typeof(T1)] = manager;
    }

    private void InitAll()
    {
        foreach (var manager in managerMap.Values)
        {
            manager.Init();
        }
    }

    private void ActiveOffAll()
    {
        foreach (var manager in managerMap.Values)
        {
            manager.ActiveOff();
        }
    }
    private void GetControllerAll()
    {
        foreach (var manager in managerMap.Values)
        {
            manager.GetController(this);
        }
    }
    private void UpdateAll()
    {
        foreach (var manager in managerMap.Values)
        {
            manager.Update();
        }
    }


    public T GetManager<T>() where T : baseManager
    {
        return (T)managerMap[typeof(T)];

    }

    void Update()
    {
        UpdateAll();
    }

    private Dictionary<Type, baseManager> managerMap = new Dictionary<Type, baseManager>();
    private Dictionary<Type, BaseScriptableObject> dicBaseScriptableObject = new Dictionary<Type, BaseScriptableObject>();

    public Transform playerTransform;
    public static GameController instance;

    [SerializeField]
    private List<BaseScriptableObject> baseScriptableObjects = new List<BaseScriptableObject>();
}