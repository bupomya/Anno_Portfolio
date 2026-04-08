# Project Structure

## Unity 버전 & 렌더 파이프라인
- URP (Universal Render Pipeline)
- Input System (New Input System)

## 외부 플러그인
- **DOTween Pro** (`Plugins/Demigiant/`) - 트윈 애니메이션 (SpeechBubble에서 사용)
- **Cinemachine** - 카메라 추적 및 카메라 쉐이크
- **TextMesh Pro** - UI 텍스트

## 폴더 구조

```
Assets/
├── Animations/
│   ├── Wizard_White/          # 플레이어 애니메이션
│   │   ├── Wizard_White.controller
│   │   ├── Idle, Walk, Jump, Fall, Landing.anim
│   │   ├── Attack1, Attack2, WalkAttack.anim
│   │   ├── Guard, Parry.anim
│   │   ├── Death, Rest.anim
│   │   └── (Animator에서 "Lock" 태그 사용 → 공격/패리 중 이동 잠금)
│   └── NPC/
│       └── Wizard/
│           ├── NPC_Wizard.controller
│           └── Idle.anim
│
├── Fonts/
│   └── MalgunGothic.ttf       # 맑은 고딕 (UI용)
│
├── Plugins/
│   └── Demigiant/             # DOTween Pro
│
├── Prefabs/                   # (현재 비어있음)
│
├── Resources/
│   └── DOTweenSettings.asset
│
├── Scenes/
│   └── GameScene.unity        # 메인 게임 씬
│
├── ScriptableObject/
│   ├── Dialogue/
│   │   └── NPC_Wizard/
│   │       ├── Hello.asset    # NPC_Wizard 첫 번째 대화
│   │       └── Bye.asset      # NPC_Wizard 두 번째 대화
│
├── Scripts/                   # ★ 핵심 스크립트 (아래 ARCHITECTURE.md 참조)
│   ├── IMovable.cs
│   ├── CharacterBase.cs
│   ├── PlayerBase.cs
│   ├── PlayerController.cs
│   ├── PlayerCombat.cs
│   ├── EnemyShooter.cs
│   ├── Projectile.cs
│   ├── Camera/
│   │   └── CameraShake.cs
│   ├── Dialogue/
│   │   ├── DialogueManager.cs
│   │   ├── DialogueData.cs
│   │   ├── DialoguePanel.cs
│   │   └── NPCInteraction.cs
│   ├── SeasonSkillType.cs
│   ├── SkillManager.cs
│   ├── Spring/
│   │   ├── GrowthInteractable.cs
│   │   ├── SplineGrowthController.cs
│   │   └── VineGrappleController.cs
│   ├── Summer/
│   │   ├── ExplosionController.cs
│   │   ├── ExplosionInteractable.cs
│   │   └── IgniteController.cs
│   ├── Autumn/
│   │   ├── LightenController.cs
│   │   ├── LightenInteractable.cs
│   │   └── GhostController.cs
│   ├── Winter/
│   │   ├── FreezeController.cs
│   │   ├── FreezeInteractable.cs
│   │   └── AnchorController.cs
│   ├── UI/
│   │   └── SeasonSkillUI.cs
│   └── Editor/
│       ├── DialogueEditorWindow.cs
│       └── CinemachineSetupTool.cs
│
├── Settings/                  # URP 렌더링 설정
│
├── Sprites/
│   ├── Wizard/               # 위자드 캐릭터 스프라이트 시트 (여러 색상)
│   ├── Platformer Assets/    # 배경, 타일, 나무, 구름 등 환경 에셋
│   └── Hp bar/               # HP 바 UI 스프라이트
│
├── TextMesh Pro/              # TMP 기본 리소스
│
└── README/                    # ★ 이 문서들
    ├── PROJECT_STRUCTURE.md
    ├── ARCHITECTURE.md
    ├── CODING_CONVENTIONS.md
    ├── DESIGN_PRINCIPLES.md
    └── WORK_LOG.md
```

## 씬 구조 (GameScene)
- Main Camera (CinemachineBrain)
- CM Camera (CinemachineCamera + CinemachineFollow + ImpulseListener)
- CameraShake (CinemachineImpulseSource)
- Player (PlayerController) - Tag: "Player", 자식에 HitPoint Transform
- NPC (NPCInteraction + Animator) - Trigger Collider로 상호작용 범위 설정
- Enemy (EnemyShooter) - firePoint Transform으로 발사 위치 지정
- 환경 오브젝트 (타일맵, 배경 등)

## 레이어 구성
- **Default** - 일반 오브젝트
- **Ground** - 지면 (플레이어 착지 판정용)
- **EnemyProjectile** - 적 투사체 (발사 시 설정)
- **ReflectedProjectile** - 패리로 반사된 투사체 (반사 시 변경)
- **Enemy** - 적 캐릭터
