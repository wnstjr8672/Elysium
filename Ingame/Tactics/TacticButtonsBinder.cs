using UnityEngine;
using UnityEngine.UI;

public class TacticButtonsBinder : MonoBehaviour
{
    [Header("Controller & Single Button")]
    public TacticsSkillController controller;   // 전술 실행 컨트롤러 (씬에 있는거)
    public Button tacticButton;                 // 전술 버튼 1개
    public Image tacticIcon;                    // 버튼 안 아이콘(Image). controller.tacticButtonImage랑 같은 거 주는 게 편함

    [Header("Specs (ScriptableObject)")]
    public TacticSpec blizzardSpec;             // Tactic_Blizzard
    public TacticSpec infernoSpec;              // Tactic_InfernoBreath

    [Header("Icons")]
    public Sprite blizzardSprite;               // 눈보라 아이콘
    public Sprite infernoSprite;                // 지옥불 숨결 아이콘
    public Sprite emptySprite;                  // 선택 없음일 때

    private void Start()
    {
        ApplyLoadout();
    }

    public void ApplyLoadout()
    {
        if (controller == null || tacticButton == null)
        {
            Debug.LogWarning("[TacticButtonsBinder] controller or tacticButton not assigned.");
            return;
        }

        string selectedTacticId = LoadoutPrefs.LoadTactic(); // "Tactic_Blizzard" / "Tactic_InfernoBreath" / ""

        tacticButton.onClick.RemoveAllListeners();

        // 전술 선택 안 됐을 때
        if (string.IsNullOrEmpty(selectedTacticId))
        {
            if (tacticIcon && emptySprite)
                tacticIcon.sprite = emptySprite;

            tacticButton.interactable = false;

            controller.currentType = default;
            controller.cooldown = 0f;
            controller.duration = 0f;
            controller.dps = 0;
            controller.damage = 0;
            controller.speed = 0f;
            controller.usesMax = 0;
            controller.usesLeft = 0;

            BindControllerUIRefsIfNeeded();
            return;
        }

        tacticButton.interactable = true;

        // ───────────────── Blizzard ─────────────────
        if (selectedTacticId == "Tactic_Blizzard")
        {
            if (tacticIcon && blizzardSprite)
                tacticIcon.sprite = blizzardSprite;

            int level = UpgradeLevelProvider.GetUpgradeLevel("UPGRADE_Tactic_Blizzard", 0);

            if (blizzardSpec != null && blizzardSpec.stats != null)
            {
                TacticStatsSnapshot s = blizzardSpec.stats.GetStats(level);

                controller.currentType = TacticType.Blizzard;
                controller.cooldown = s.cooldown;
                controller.duration = s.duration; // 블리자드는 실제 사용 시 SpineBlizzardEffect가 다시 계산하지만 들고만 있어도 됨
                controller.dps = s.dps;
                controller.damage = 0;
                controller.speed = 0f;
                controller.usesMax = s.uses;
                controller.usesLeft = s.uses;
            }
            else
            {
                Debug.LogWarning("[TacticButtonsBinder] Blizzard spec/stats not assigned");
            }

            tacticButton.onClick.AddListener(controller.OnClickBlizzard);
        }
        // ───────────────── InfernoBreath ─────────────────
        else if (selectedTacticId == "Tactic_InfernoBreath")
        {
            if (tacticIcon && infernoSprite)
                tacticIcon.sprite = infernoSprite;

            int level = UpgradeLevelProvider.GetUpgradeLevel("UPGRADE_Tactic_InfernoBreath", 0);

            if (infernoSpec != null && infernoSpec.stats != null)
            {
                TacticStatsSnapshot s = infernoSpec.stats.GetStats(level);

                controller.currentType = TacticType.InfernoBreath;
                controller.cooldown = s.cooldown;
                controller.duration = 0f;
                controller.dps = 0;
                controller.damage = s.damage;
                controller.speed = s.speed;
                controller.usesMax = s.uses;
                controller.usesLeft = s.uses;
            }
            else
            {
                Debug.LogWarning("[TacticButtonsBinder] Inferno spec/stats not assigned");
            }

            tacticButton.onClick.AddListener(controller.OnClickInfernoBreath);
        }
        // ───────────────── Unknown Tactic ─────────────────
        else
        {
            if (tacticIcon && emptySprite)
                tacticIcon.sprite = emptySprite;

            tacticButton.interactable = false;

            controller.currentType = default;
            controller.cooldown = 0f;
            controller.duration = 0f;
            controller.dps = 0;
            controller.damage = 0;
            controller.speed = 0f;
            controller.usesMax = 0;
            controller.usesLeft = 0;
        }

        BindControllerUIRefsIfNeeded();
    }

    private void BindControllerUIRefsIfNeeded()
    {
        if (controller.tacticButton == null)
            controller.tacticButton = tacticButton;

        if (controller.tacticButtonImage == null && tacticIcon != null)
            controller.tacticButtonImage = tacticIcon;
    }

#if UNITY_EDITOR
    [ContextMenu("Re-Apply Loadout (Editor Preview)")]
    private void __EditorApply() => ApplyLoadout();
#endif
}
