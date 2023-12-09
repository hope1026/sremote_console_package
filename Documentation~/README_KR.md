# SPlugin Remote Console for Unity

- - -
## Table of contents
* [Introduction](#introduction)
* [Features](#features)
* [Getting started](#getting-started)

- - -
## Introduction
RemoteConsole 은 원격으로 전송 되는 로그를 통해 디버깅을 하고, 사용자 정의 명령어를 이용하여 개발 편의성을 높이는 강력한 툴입니다.
* 유니티 **기본 콘솔의 모든 기능**을 지원합니다.
* 빌드 된 앱과 **원격** 으로 연결되어 **사용자 정의 명령어** 를 실행합니다.
* 빌드 된 앱과 **원격** 으로 연결되어 **로그** 를 확인할 수 있습니다.
* **원격** 으로 연결 된 앱에 **Pause, Step** 기능을 이용하여, 프레임 단위 실행이 가능합니다.
* 로그 발생 시점의 **시간**, **프레임 카운트**, **오브젝트** 를 쉽게 확인 가능합니다.
* 편리한 로그 **필터** 기능을 제공합니다.
* 연결된 앱의 **시스템 정보** 및 간단한 **프로파일**(FPS, UsedHeap) 정보를 확인 가능합니다
>[![log_code.png](Images%2Fremote_console_thumbnail.png)](https://www.youtube.com/watch?v=Da6OSc6FiX8)

- - -
## Features
### 사용자 정의 명령어
플레이 중 각종 값을 변경 가능하며 테스트나 디버깅에 유용합니다.
1. 스크립트에서 SPlugin.SCommand.Register 함수를 이용하여 사용자 정의 명령어를 등록합니다
   * SPlugin.Command.Register(categoryName, commandName, defaultValue, onChangedValueDelegate, displayPriority, tooltip)
2. SConsole 에디터의 CommandView 에서 등록된 사용자 정의 명령어를 확인 가능합니다.
3. 사용자 정의 명령어의 값을 변경합니다.
>![register_commands_code.png](Images%2Fregister_commands_code.png)
>![apply_commands_to_remote_app.gif](Images%2Fapply_commands_to_remote_app.gif)
- - - 

### 로그
* UnityEngine.Debug 이용
  1. 스크립트에서 UnityEngine.Debug.Log 함수를 이용하여 로그를 작성합니다.
     * UnityEngine.Debug.Log("log")
  2. SConsole 에디터의 Preferences 에서 Show UnityDebugLog 를 활성화 합니다.
  3. SConsole 에디터의 LogView에서 로그 확인이 가능합니다.
* SPlugin.SDebug 이용
  1. 스크립트에서 SPlugin.SDebug.Log 함수를 이용하여 로그를 작성합니다.
     * SPlugin.SDebug.Log("log")
  2. SConsole 에디터의 LogView에서 로그 확인이 가능합니다.
>![log_code.png](Images%2Flog_code.png)
>![show_log_from_remote_app.gif](Images%2Fshow_log_from_remote_app.gif)
- - -

### 편리한 필터 기능
* Search 를 이용하여 로그 찾기
* Exclude 를 이용하여 로그 제외하기
* QuickSearch 를 이용하여 로그 찾기
>![quick_search.gif](Images%2Fquick_search.gif)
- - -

###  Pause, Step
원격으로 연결된 앱에서 프레임 단위 플레이가 가능하여 디버깅에 도움이 됩니다.
>![remote_app_pause_and_step.gif](Images%2Fremote_app_pause_and_step.gif)
- - -

### 원격 접속
* 로컬 네트워크에서 접속 가능한 앱을 찾아 연결
  1. SConsole 에디터의 ApplicationView 에서 접속이 가능한 앱을 찾습니다.
  2. 연결 가능한 앱이 있을 경우 리스트에 등록 됩니다.
  3. 앱을 선택 합니다.
* 로컬 네트워크의 사설 IP 또는 공인 IP 를 이용하여 앱 연결
  1. SConsole 에디터의 ApplicationView 에서 **로컬 네트워크의 사설 IP** 또는 **공인 IP**를 입력합니다.
  2. Connect 버튼을 눌러 연결 합니다*
>![connect_to_remote_app.gif](Images%2Fconnect_to_remote_app.gif)
- - -

### 시스템 및 프로파일 정보
1. SConsole 에디터의 ApplicationView 에서 앱 리스트를 확인합니다.
2. ShowInfo 버튼을 눌러 시스템 및 프로파일 정보를 확인합니다.
>![systeminfo.png](Images%2Fsysteminfo.png)
- - -

## Getting started
1. SRemoteConsole 설치
   1. 유니티 에디터에서 `Window/Package Manager` 를 클릭 합니다.
   2. Package Manager 의 왼쪽 상단 `+` 버튼을 클릭하고 `Add package from git URL...` 을 선택합니다.
   3. SRemoteConsole 패키지의 git 주소를 입력 합니다.
           `https://github.com/hope1026/unity_s_remote_console_package.git`
   4. `Add` 버튼을 클릭하면 패키지가 설치 됩니다.
2. SRemoteConsole 실행
   1. 유니티 에디터에서 `Window/SPlugin/SConsole` 를 클릭 합니다.
3. 스크립트에 로그를 작성합니다
   1. SPlugin.SDebug.Log("log");
4. SConsole 에디터에서 로그를 확인 합니다.