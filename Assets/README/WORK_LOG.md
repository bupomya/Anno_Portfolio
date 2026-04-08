# Work Log

> 주요 작업 이력을 기록합니다. (최신순)
> 각 항목은 **무엇을**, **왜** 변경했는지 기록합니다.

---

## 2026-03-30 | 겨울 스킬 구현 - Freeze + Anchor

### SeasonSkillType enum 변경
- **Ice 제거 → Freeze, Anchor 추가**: 4개 계절 모두 좌클릭/우클릭 2개 스킬 체계 완성
- SkillManager 기본 바인딩: 겨울(Freeze+Anchor)

### Freeze 스킬 신규 구현 (겨울 좌클릭)
- **FreezeController (싱글톤)**: MP 체크 후 오브젝트에 결빙 효과 적용
  - 1회 고정 MP 비용 (mpCost)
- **FreezeInteractable**: 맵 오브젝트에 부착하는 컴포넌트
  - 좌클릭 시 velocity 초기화 + gravityScale=0 + FreezeAll (완전 정지)
  - 청/하얀색 틴트로 시각적 피드백
  - duration 후 깜박임 경고 → 원래 상태 복원 (LightenInteractable 패턴 재활용)

### Anchor 스킬 신규 구현 (겨울 우클릭)
- **AnchorController (싱글톤)**: 우클릭 홀드로 플레이어 위치 고정
  - velocity 초기화 + gravityScale=0 + FreezeAll → 공중/지상 모두 사용 가능
  - 청/하얀색 틴트로 시각적 피드백
  - 초당 MP 소모 (mpCostPerSecond), MP 부족 시 강제 해제
  - 활용: 이동 플랫폼 타이밍 맞추기, 바람/빙판 맵에서 위치 고정

---

## 2026-03-30 | PlayerState SO 제거 및 PlayerBase 직접 통합

### PlayerState ScriptableObject 제거
- **PlayerState.cs 삭제**: 계절별 SO 분리가 불필요 (ChangeSeasonState 미사용, MP 값 동일)
- **4개 .asset 파일 삭제**: SpringStats, SummerStats, AutumnStats, WinterStats
- 밸런싱은 각 스킬 컨트롤러의 수치로 관리하는 구조로 단순화

### PlayerBase에 스탯 직접 통합
- moveSpeed, jumpForce, gravityScale, maxMp, mpRegenInterval, mpRegenAmount → SerializeField로 직접 선언
- seasonStates[], ChangeState(), ChangeSeasonState(), CurrentState 프로퍼티 제거
- MaxMp, JumpForce public getter 추가 (스킬 컨트롤러/UI에서 접근)

### 스킬 컨트롤러 6개 + UI 수정
- `player.CurrentState == null` 체크 → `player == null`로 변경
- VineGrappleController: `player.CurrentState.jumpForce` → `player.JumpForce`
- SeasonSkillUI: `player.CurrentState.maxMp` → `player.MaxMp`

---

## 2026-03-30 | 봄/여름 우클릭 스킬 추가 - VineGrapple + Ignite

### SeasonSkillType enum 확장
- **VineGrapple, Ignite 추가**: 봄/여름에도 가을과 동일하게 좌클릭(오브젝트 상호작용) + 우클릭(플레이어 효과) 2개 스킬 체계 완성
- SkillManager 기본 바인딩: 봄(Growth+VineGrapple), 여름(Explosion+Ignite)

### PlayerBase에 이동 배율 프로퍼티 추가
- **SpeedMultiplier, JumpMultiplier**: Ignite가 조작하는 이동속도/점프력 배율 (기본 1f)
- FixedUpdate에서 `moveSpeed * SpeedMultiplier`, `jumpForce * JumpMultiplier` 적용
- SO 데이터를 직접 수정하지 않는 안전한 방식

### VineGrapple 스킬 신규 구현 (봄 우클릭)
- **VineGrappleController (싱글톤)**: 우클릭으로 마우스 방향 덩굴 발사
  - Physics2D.Raycast로 벽/천장 감지 → 히트 시 플레이어를 해당 지점으로 당김
  - LineRenderer로 덩굴 시각화 (플레이어 ~ 히트 포인트)
  - 이동 중 MovementLocked = true, 도착/재클릭 시 해제
  - 1회 고정 MP 비용 (mpCost)

### Ignite 스킬 신규 구현 (여름 우클릭)
- **IgniteController (싱글톤)**: 우클릭으로 점화 부스트 토글
  - SpeedMultiplier/JumpMultiplier로 이동속도 2배, 점프력 1.5배
  - 스프라이트 주황색 틴트로 시각적 피드백
  - 초당 MP 소모 (mpCostPerSecond), MP 부족 시 강제 해제
  - GhostController와 동일한 토글 패턴

---

## 2026-03-29 | 스킬 시스템 리팩토링 - 계절 해금 방식 + MP 통합

### currentMp를 PlayerBase로 이동
- **PlayerState.currentMp 제거**: SO의 런타임 값 → PlayerBase의 `CurrentMp` 프로퍼티로 이동
- ChangeState() 시 MP가 리셋되지 않고 유지됨 (새 maxMp로 클램프만)
- 모든 스킬 컨트롤러: `player.CurrentState.currentMp` → `player.CurrentMp` 로 변경

### PlayerState에서 availableSkills 제거
- **availableSkills 필드, HasSkill() 메서드 제거**: 스킬 중복 문제 해결
- PlayerState는 이동/물리/MP 설정만 보유하는 순수 데이터 SO로 정리

### SkillManager에 계절 해금 시스템 추가
- **bool[] unlockedSeasons**: Inspector 체크박스로 해금 상태 관리
- **UnlockSeason(int)**: 게임 시스템이 호출하는 해금 API
- **IsSeasonUnlocked(int)**: 해금 여부 조회
- **OnSeasonUnlocked 이벤트**: UI/사운드 등 반응용
- SetActiveSeason()이 HasSkill() 대신 unlockedSeasons[] 체크
- player 참조 제거 (더 이상 불필요)

### PlayerBase에 seasonStates 배열 추가
- **PlayerState[] seasonStates**: Inspector에서 4개 계절 SO 할당 (0=봄~3=겨울)
- **ChangeSeasonState(int)**: 계절 인덱스로 SO 전환

---

## 2026-03-29 | 가을 스킬 - Lighten (무게 경감) + Ghost (유체화) 구현 + SkillManager 개선

### SeasonSkillType enum 변경
- **Wind → Lighten 변경, Ghost 추가**: Growth, Explosion, Lighten, Ghost, Ice
- 가을에 두 개의 스킬 할당 (Lighten + Ghost)

### SkillManager 계절 단위 선택으로 변경
- **SeasonKeyBinding 구조체 도입**: 계절별로 스킬을 그룹화하여 Inspector에서 관리
  - seasonName (계절 이름), key (단축키), skills[] (스킬 목록)
- 기본 키 바인딩: 1=봄(Growth), 2=여름(Explosion), 3=가을(Lighten+Ghost), 4=겨울(Ice)
- 개별 스킬 선택 → 계절 선택으로 변경. 한 계절의 모든 스킬이 동시 활성화
- 입력 구분은 각 스킬 컨트롤러가 처리 (좌클릭=Lighten, 우클릭=Ghost)

### Lighten 스킬 신규 구현
- **LightenController (싱글톤)**: MP 체크 후 오브젝트에 경감 효과 적용
  - 1회 고정 MP 비용 (mpCost), gravityMultiplier/massMultiplier로 경감 정도 조절
- **LightenInteractable**: 맵 오브젝트에 부착하는 컴포넌트
  - 좌클릭 시 Rigidbody2D constraints 해제 + 중력/질량 경감
  - duration 후 원래 값 자동 복원, 복원 시 velocity 초기화

### Ghost 스킬 신규 구현
- **GhostController (싱글톤)**: 우클릭으로 고스트 모드 토글
  - 플레이어 알파값 감소 (반투명 시각 효과)
  - Physics2D.IgnoreLayerCollision으로 GhostWall 레이어 통과
  - 초당 MP 소모 (mpCostPerSecond), MP 부족 시 강제 해제
  - 안전 위치 추적: GhostWall 바깥에 있을 때만 safePosition 갱신
  - 해제 시 GhostWall 내부에 있으면 safePosition으로 텔레포트
  - OverlapCapsule로 플레이어 콜라이더 기반 내부 판정

### AutumnStats/WinterStats 에셋 업데이트
- AutumnStats: 마나 필드 추가, availableSkills에 Growth/Explosion/Lighten/Ghost 할당
- WinterStats: 마나 필드 추가, availableSkills에 전체 5개 스킬 할당

---

## 2026-03-26 | 여름 스킬 - Explosion (폭발) 구현 + 이동 플랫폼 시스템 + MP 비용 리팩토링

### Explosion 스킬 신규 구현
- **ExplosionController (싱글톤)**: 차징 → 폭발 → 오브젝트 발사 관리
  - 마우스 꾹 누르면 폭발 범위/위력이 chargeRatio(0~1)에 따라 증가
  - 놓으면 OverlapCircleAll로 범위 내 ExplosionInteractable 탐색 → 중심에서 바깥으로 AddForce(Impulse)
  - upwardBias로 상향 보정 → 물리 기반 포물선 궤적
  - 1회 고정 MP 비용 (mpCostPerCharge), CameraShake 연동
  - 차징 인디케이터 프리팹으로 범위 시각화
- **ExplosionInteractable**: 폭발에 반응하는 오브젝트 컴포넌트
  - Dynamic Rigidbody2D + FreezeAll constraints (정지 상태)
  - Launch() 시 constraints 해제 → AddForce → 포물선 비행
  - 착지 판정: 바닥 충돌(normal.y > 0.5) 또는 속도 임계값 이하
  - 착지 시 FreezeAll 복원, destroyDelay로 선택적 파괴

### 이동 플랫폼 시스템 (PlayerBase)
- GroundCheck에서 밟은 Collider2D의 attachedRigidbody 속도를 플레이어 속도에 합산
- 범용 시스템: ExplosionInteractable뿐 아니라 Rigidbody2D를 가진 모든 이동 플랫폼에 적용
- 비행 중 점프로 자유롭게 이탈 가능
- GroundCheckOffset, GroundCheckSize public getter 추가

### MP 비용 관리 리팩토링
- PlayerState에서 mpCostPerUnit 제거 → SplineGrowthController로 이동
- 원칙: 스킬별 비용은 해당 스킬 컨트롤러의 SerializeField로 관리. PlayerState는 공통 필드만 보유

---

## 2026-03-24 | Growth 스킬 시스템 구현 및 계절별 스킬 제한

### Growth 스킬 (SplineGrowthController + GrowthInteractable)
- **Spline 기반 식물 성장**: 마우스 드래그로 SpriteShape Spline을 실시간 생성
- **Prefab 방식**: 런타임 AddComponent 대신 에디터에서 설정된 SpriteShape Prefab을 Instantiate
  - Profile, Material, Open Ended, Height 등을 Prefab에서 미리 설정
- **발판 기능**: 성장 완료 시 EdgeCollider2D 추가 → Ground 레이어로 플랫폼 역할
- **수축/소멸**: shrinkDelay 후 끝점→시작점 방향으로 줄어들며 shrinkDuration 후 Destroy

### MP 시스템
- **PlayerState 확장**: mp → maxMp/currentMp 분리, mpRegenInterval/mpRegenAmount 추가
- **자동 회복**: PlayerBase.Update()에서 일정 간격마다 MP 충전 (maxMp 초과 불가)
- **Growth MP 소모**: 거리 × mpCostPerUnit, MP 부족 시 자동 성장 중단

### 계절별 스킬 제한
- **SeasonSkillType enum 생성**: Growth, Fire, Wind, Ice
- **PlayerState에 availableSkills 추가**: 계절별 SO에서 사용 가능한 스킬 목록 관리
- **HasSkill() 체크**: SplineGrowthController.StartGrowth()에서 Growth 스킬 보유 여부 확인
- SpringStats에만 Growth 할당 → 다른 계절에서는 사용 불가

---

## 2026-03-23 | 대화 시스템 패널 방식으로 전면 교체
- **SpeechBubble → DialoguePanel**: 말풍선 방식에서 화면 하단 패널 방식으로 변경
  - Screen Space Overlay 캔버스, 왼쪽 Player 초상화 / 오른쪽 NPC 초상화
  - 말하는 캐릭터 강조(밝게), 상대방 어둡게 처리
  - 화자 이름 표시 + DOTween 타이핑 효과
- **DialogueManager 수정**: playerName, playerPortrait 필드 추가, DialoguePanel 사용
- **DialogueData 수정**: BubbleStyle 제거, typingSpeed만 유지
- **NPCInteraction 수정**: npcPortrait 필드 추가, SpeechBubble 의존 제거
- **DialogueEditorWindow 수정**: BubbleStyle 섹션 제거, 패널 스타일 프리뷰로 변경
- **SpeechBubble.cs**: 더 이상 미사용 (삭제 가능)

---

## 2026-03-23 | PlayerState SO 도입 (계절별 이동 파라미터)
- **PlayerState SO 생성**: moveSpeed, jumpForce, gravityScale을 SO로 분리
  - `[CreateAssetMenu]`로 에셋 생성 메뉴 등록 (Player/Player State)
  - 계절별 에셋 생성 필요: SpringStats, SummerStats, AutumnStats, WinterStats
- **PlayerBase 수정**: 하드코딩된 moveSpeed/jumpForce 제거, PlayerState SO에서 읽도록 변경
  - `ChangeState(PlayerState)` 메서드로 런타임 SO 교체 지원
  - Awake에서 초기 gravityScale 적용

---

## 2026-03-23 | PlayerController 책임 분리 리팩토링
- **IMovable 인터페이스 생성**: 이동/점프 추상화 (IsGrounded, SetMoveInput, RequestJump)
  - 다른 캐릭터 타입(AI 적, NPC 등)에서 동일한 이동 계약 재사용 가능
- **PlayerBase 수정**: IMovable 구현, 입력 읽기를 `ReadInput()` 추상 메서드로 분리
  - `MovementLocked` 프로퍼티 추가 (외부 시스템이 이동 잠금)
  - `ResetIdleTimer()` 공개 (전투 입력 시 휴식 타이머 초기화)
  - `IsLocked()` public으로 변경
  - 기존 가상 메서드 제거 (HandleInput, HasAdditionalInput, CanMove, OnUpdate)
- **PlayerController 단순화**: 이동/점프 입력만 처리 (ReadInput → SetMoveInput/RequestJump)
- **PlayerCombat 컴포넌트 생성**: 공격/가드/패리 로직을 별도 MonoBehaviour로 분리
  - PlayerBase 참조로 IsGrounded, FacingDirection, MovementLocked 연동
- **Projectile 수정**: PlayerController 대신 PlayerCombat + PlayerBase 참조

---

## 2026-03-01 | README 문서 구조 생성
- `Assets/README/` 폴더에 프로젝트 문서화
  - `PROJECT_STRUCTURE.md` - 폴더/파일 구조
  - `ARCHITECTURE.md` - 시스템 설계 및 의존성
  - `CODING_CONVENTIONS.md` - 코딩 규칙
  - `WORK_LOG.md` - 작업 이력 (이 파일)
- Claude Code 작업 효율화를 위해 프로젝트 전체를 탐색하지 않고 README만 참조할 수 있도록 정리

---

## 초기 커밋 ~ 현재 | 기존 구현 요약
- **캐릭터 시스템**: CharacterBase → PlayerBase → PlayerController 상속 구조
- **전투 시스템**: EnemyShooter의 투사체 발사 → 플레이어 가드/패리 → 투사체 반사 → 적 처치
- **대화 시스템**: DialogueData(SO) + NPCInteraction + DialogueManager + SpeechBubble
- **카메라 시스템**: Cinemachine + CameraShake (공격/피격/패리 시 흔들림)
- **에디터 도구**: DialogueEditorWindow (대화 SO 생성 GUI), CinemachineSetupTool (카메라 자동 세팅)

---

<!-- 새 작업 기록은 이 위에 추가 -->
