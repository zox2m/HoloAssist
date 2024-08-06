using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Video;
using System.IO;

public class PopupUIManager : MonoBehaviour
{
    public PopupUI _APopup;
    public PopupUI _BPopup;

    [Space]
    public KeyCode _escKey    = KeyCode.Escape;
    public KeyCode _AKey = KeyCode.A;
    public KeyCode _BKey     = KeyCode.B;

    /// 실시간 팝업 관리 링크드 리스트 
    private LinkedList<PopupUI> _activePopupLList;

    /// 전체 팝업 목록 
    private List<PopupUI> _allPopupList;

    private Text_Manager tm;
    private Video_Manager vm;

    private void Start()
    {
        _activePopupLList = new LinkedList<PopupUI>();
        Init();
        InitCloseAll();
    }

    /*
    private void Update()
    {
        // ESC 누를 경우 링크드리스트의 First 닫기
        if (Input.GetKeyDown(_escKey))
        {
            if (_activePopupLList.Count > 0)
            {
                ClosePopup(_activePopupLList.First.Value);
            }
        }

        // 단축키 조작
        ToggleKeyDownAction(_AKey, _APopup);
        ToggleKeyDownAction(_BKey, _BPopup);
    }*/

    private void Init()
    {
        tm = new Text_Manager();
        vm = new Video_Manager(); 

        // 1. 리스트 초기화
        _allPopupList = new List<PopupUI>()
        {
            _APopup, _BPopup
        };

        // 2. 모든 팝업에 이벤트 등록
        foreach (var popup in _allPopupList)
        {
            // 헤더 포커스 이벤트
            popup.OnFocus += () =>
            {
                _activePopupLList.Remove(popup);
                _activePopupLList.AddFirst(popup);
                RefreshAllPopupDepth();
            };

            // 닫기 버튼 이벤트
            popup._closeButton.onClick.AddListener(() => ClosePopup(popup));
        }
    }

    /// <summary> 시작 시 모든 팝업 닫기 </summary>
    private void InitCloseAll()
    {
        foreach (var popup in _allPopupList)
        {
            ClosePopup(popup);
        }
    }
    /// <summary> 단축키 입력에 따라 팝업 열거나 닫기 </summary>
    public void ToggleKeyDownAction(int popup_index, string targe_name)
    {
        ToggleOpenClosePopup(_allPopupList[popup_index] ,targe_name);
    }

    /// <summary> 팝업의 상태(opened/closed)에 따라 열거나 닫기 </summary>
    private void ToggleOpenClosePopup(PopupUI popup, string targe_name)
    {
        if (!popup.gameObject.activeSelf) OpenPopup(popup, targe_name);
        else ClosePopup(popup);
    }

    /// <summary> 팝업을 열고 링크드리스트의 상단에 추가 </summary>
    private void OpenPopup(PopupUI popup, string targe_name)
    {
        _activePopupLList.AddFirst(popup);
        popup.gameObject.transform.GetChild(0).gameObject.GetComponent<VideoPlayer>().clip = vm.get_Video(targe_name);
        popup.gameObject.transform.GetChild(2).gameObject.GetComponent<TextMeshProUGUI>().text = tm.get_Text(targe_name);
        RefreshAllPopupDepth();
    }

    /// <summary> 팝업을 닫고 링크드리스트에서 제거 </summary>
    private void ClosePopup(PopupUI popup)
    {
        _activePopupLList.Remove(popup);
        popup.gameObject.SetActive(false);
        RefreshAllPopupDepth();
    }

    /// <summary> 링크드리스트 내 모든 팝업의 자식 순서 재배치 </summary>
    private void RefreshAllPopupDepth()
    {
        foreach (var popup in _activePopupLList)
        {
            popup.transform.SetAsFirstSibling();
        }
    }
}