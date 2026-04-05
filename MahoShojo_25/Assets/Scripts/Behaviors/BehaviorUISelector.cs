using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BehaviorUISelector : MonoBehaviour
{
    /// <summary>
    /// goes on the UI manager and grabs all active UI
    /// </summary>
    // Start is called before the first frame update

    private List<Transform> allInteractiveUI = new List<Transform>(); // Component: Buttons |or| Sliders
    public List<Transform> activeUI = new List<Transform>();
    public int uiSelectionId { get; private set; } = 0;
    private Button activeButton = null;
    private Slider activeSlider = null;
    private float stampUpdate;

    private void Awake()
    {
        GrabAllSelectableUI();  
    }

    private void GrabAllSelectableUI()
    {
        // grab all ui transforms if Button Or Slider
        AddAllInteractiveUiChildren(transform);
    }

    private void AddAllInteractiveUiChildren(Transform _parent)
    {
        for( int i = 0; i < _parent.childCount; i++)
        {
            if (_parent.GetChild(i).GetComponent<Button>() || _parent.GetChild(i).GetComponent<Slider>())
                allInteractiveUI.Add(_parent.GetChild(i));

            if (_parent.GetChild(i).childCount > 0)
                AddAllInteractiveUiChildren(_parent.GetChild(i));
        }
    }

    private void Start()
    {
        UpdateActiveUiList();
    }

    //private void Update() // REMOVE THESE when plugged into other scripts
    //{
    //    if (Input.GetKeyDown(KeyCode.W) ) ChangeUiId(uiSelectionId - 1);
    //    if (Input.GetKeyDown(KeyCode.S) ) ChangeUiId(uiSelectionId + 1);
    //    if (Input.GetKey(KeyCode.D) ) ChangeSliderDirection(true);
    //    if (Input.GetKey(KeyCode.A) ) ChangeSliderDirection(false);
    //    if (Input.GetKeyDown(KeyCode.Space)) SelectButton();

    //}

    public void UpdateActiveUiList() // call anytime a menu changes and buttons show up
    {
        if (allInteractiveUI.Count == 0) return;

        activeUI.Clear();

        for (int i = 0; i < allInteractiveUI.Count; i++)
        {
            if (allInteractiveUI[i].gameObject.activeInHierarchy == false) continue;

            if (allInteractiveUI[i].gameObject.activeSelf == true)
            { activeUI.Add(allInteractiveUI[i]); }
        }

        activeButton = null;
        activeSlider = null;
        ChangeUiId(0);
    }

    public void ChangeUiId(int _newId) // call when menu is active and we press Up or Down buttons
    {
        //print($"Attempting to change ui id from: {uiSelectionId} to -  {_newId}");
        if (activeUI.Count == 0) return;

        ChangeUiSelection(null); // remove any selection
        
        if (_newId >= activeUI.Count) _newId = 0;
        if (_newId < 0) _newId = activeUI.Count - 1;

        for (int i = 0; i < activeUI.Count; i++)
        {
            if (i != _newId) continue;
            //print($"Found Matching UI: {activeUI[i].name}");
            activeUI[i].TryGetComponent(out activeButton);
            activeUI[i].TryGetComponent(out activeSlider);

            if (activeButton) { ChangeUiSelection(activeButton.gameObject); activeButton.Select(); }
            if (activeSlider) activeSlider.Select();
            uiSelectionId = _newId;
            break;
        }
    }

    public void ChangeSliderDirection(bool moveRight) // call when we press left or right
    {
        //print($"Attempting to change slider: {moveRight}");
        if (!activeSlider || activeSlider.gameObject.activeInHierarchy == false) return;

        float amount = 0.01f * (Mathf.Abs(activeSlider.maxValue) + Mathf.Abs(activeSlider.minValue));
        if (!moveRight) amount *= -1f;
        //float newValue = activeSlider.value + amount;
        activeSlider.value += amount;//newValue;
    }

    public void ChangeUiSelection(GameObject _obj)
    {
        EventSystem.current.SetSelectedGameObject(_obj);
    }

    public void SelectButton() // call when we press any select/confirm button
    {
        //print($"Attempting to select button");
        if (!activeButton || activeButton.gameObject.activeInHierarchy == false) { ChangeUiSelection(null); UpdateActiveUiList(); }// return; }
        else { print($"Button Selected = #{uiSelectionId} - {activeUI[uiSelectionId].name} \n Which should match: {activeButton.gameObject.name}"); activeButton.onClick.Invoke(); }// if the button is still active then press it
        UpdateActiveUiList();       
    }

}

