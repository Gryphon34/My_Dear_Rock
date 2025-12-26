My Dear Rock

실제 서비스가 가능한 수준의 백엔드 연동과 수익화 모델을 경험해 보는 것을 목표로 진행.

데이터가 사라지지 않는 위해 Firebase Realtime Database를 연동했습니다.

보안 규칙에서 $uid === auth.uid를 적용해, 오직 본인만 자신의 데이터를 수정할 수 있도록 설계

테마, 이펙트, 아이템 등 다양한 상점 리스트를 일일이 하드코딩하지 않고, **JSON 직렬화(Serialization)**를 사용해 한 번에 클라우드와 통신하도록 구현

새로운 아이템이 추가되어도 코드 수정 없이 유연하게 대응할 수 있는 구조.

하단 배너 광고와, 유저가 선택하여 보상을 얻을 수 있는 보상형 광고를 AdMob으로 구현

광고 시청 완료 시 조약돌 500개를 즉시 지급하는 로직

구형 방식 대신 유니티의 New Input System을 도입하여 모바일 환경의 터치 반응성을 높임

ThemeSettings를 통해 테마 변경 시 스카이박스, 조명, BGM이 한꺼번에 교체되는 시스템을 구축하여 최적화

프로젝트 구성 (Tech Stack)
Engine: Unity 6 (6000.0.30f1)

Language: C#

Backend: Firebase (Google/Anonymous Auth, Realtime Database)

Ads: Google AdMob

VCS: Git, GitHub Desktop (LFS 활용)
