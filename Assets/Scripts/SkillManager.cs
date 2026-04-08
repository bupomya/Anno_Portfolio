using System;
using UnityEngine;

public class SkillManager : MonoBehaviour
{
    [Serializable]
    public struct SeasonKeyBinding
    {
        [Tooltip("계절 이름 (Inspector 식별용)")]
        public string seasonName;

        [Tooltip("이 계절을 활성화/해제하는 단축키")]
        public KeyCode key;

        [Tooltip("이 계절에 포함된 스킬 목록 (좌클릭/우클릭 등 입력은 각 컨트롤러가 처리)")]
        public SeasonSkillType[] skills;
    }

    public static SkillManager Instance { get; private set; }

    [Header("Season Bindings")]
    [Tooltip("계절별 단축키 및 스킬 그룹. 같은 키를 다시 누르면 해제")]
    [SerializeField] private SeasonKeyBinding[] seasonBindings = new SeasonKeyBinding[]
    {
        new SeasonKeyBinding { seasonName = "봄", key = KeyCode.Alpha1, skills = new[] { SeasonSkillType.Growth, SeasonSkillType.VineGrapple } },
        new SeasonKeyBinding { seasonName = "여름", key = KeyCode.Alpha2, skills = new[] { SeasonSkillType.Explosion, SeasonSkillType.Ignite } },
        new SeasonKeyBinding { seasonName = "가을", key = KeyCode.Alpha3, skills = new[] { SeasonSkillType.Lighten, SeasonSkillType.Ghost } },
        new SeasonKeyBinding { seasonName = "겨울", key = KeyCode.Alpha4, skills = new[] { SeasonSkillType.Freeze, SeasonSkillType.Anchor } },
    };

    [Header("Unlock State")]
    [Tooltip("각 계절의 잠금 해제 여부. Season Bindings와 동일한 인덱스 (0=봄, 1=여름, 2=가을, 3=겨울)")]
    [SerializeField] private bool[] unlockedSeasons;

    private int activeSeasonIndex = -1;

    public int ActiveSeasonIndex => activeSeasonIndex;
    public int SeasonCount => seasonBindings.Length;

    public event Action<int> OnSeasonUnlocked;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (unlockedSeasons == null || unlockedSeasons.Length != seasonBindings.Length)
        {
            unlockedSeasons = new bool[seasonBindings.Length];
            unlockedSeasons[0] = true;
        }
    }

    private void Update()
    {
        for (int i = 0; i < seasonBindings.Length; i++)
        {
            if (Input.GetKeyDown(seasonBindings[i].key))
            {
                if (activeSeasonIndex == i)
                    ClearActiveSeason();
                else
                    SetActiveSeason(i);

                break;
            }
        }
    }

    private void SetActiveSeason(int index)
    {
        if (!IsSeasonUnlocked(index)) return;

        activeSeasonIndex = index;
    }

    public void ClearActiveSeason()
    {
        activeSeasonIndex = -1;
    }

    public bool IsActiveSkill(SeasonSkillType skill)
    {
        if (activeSeasonIndex < 0 || activeSeasonIndex >= seasonBindings.Length) return false;

        SeasonSkillType[] skills = seasonBindings[activeSeasonIndex].skills;
        return Array.IndexOf(skills, skill) >= 0;
    }

    public void UnlockSeason(int seasonIndex)
    {
        if (seasonIndex < 0 || seasonIndex >= unlockedSeasons.Length) return;
        if (unlockedSeasons[seasonIndex]) return;

        unlockedSeasons[seasonIndex] = true;
        OnSeasonUnlocked?.Invoke(seasonIndex);
    }

    public bool IsSeasonUnlocked(int seasonIndex)
    {
        if (seasonIndex < 0 || seasonIndex >= unlockedSeasons.Length) return false;
        return unlockedSeasons[seasonIndex];
    }
}
