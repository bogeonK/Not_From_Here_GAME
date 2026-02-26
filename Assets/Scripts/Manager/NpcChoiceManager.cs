using System.Collections.Generic;
using UnityEngine;

public class NpcChoiceManager : baseManager
{
    private NpcChoiceConfigSO _config;
    private NpcChoicePopupUI_JRPG _currentUI;

    public NpcChoiceManager(NpcChoiceConfigSO config)
    {
        _config = config;
    }

    public override void GetController(GameController controller) { }

    public override void Init() { }

    public override void ActiveOff() { }

    public override void Update() { }

    public override void Destory()
    {
        if (_currentUI != null)
            GameObject.Destroy(_currentUI.gameObject);
    }

    public void OpenChoice(NPCInteractable npc, NpcProfileSO profile)
    {
        if (_currentUI != null) return;

        if (_config == null || _config.popupPrefab == null)
        {
            return;
        }

        var go = GameObject.Instantiate(_config.popupPrefab);

        _currentUI = go.GetComponent<NpcChoicePopupUI_JRPG>(); 
        if (_currentUI == null)
        {
            GameObject.Destroy(go);
            return;
        }

        _currentUI.Open(
            new List<string> { "죽인다", "안 죽인다" },
            (idx) =>
            {
                if (idx == 0)
                {
                    npc.ResolveChoice(NpcChoice.Kill);
                    Close();
                    GameController.instance.DialogueUI.Close();
                }
                else
                {
                    npc.ResolveChoice(NpcChoice.Spare);
                    Close();
                    GameController.instance.DialogueUI.ShowExitExcuseAndClose(npc);
                }
            },
            onCancel: () =>
            {
                Close();
                GameController.instance.DialogueUI.Close();
            },
            prompt: "어떻게 할까?"
        );


    }

    private void Close()
    {
        if (_currentUI == null) return;
        GameObject.Destroy(_currentUI.gameObject);
        _currentUI = null;
    }
}
