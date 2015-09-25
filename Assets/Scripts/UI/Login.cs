using UnityEngine;
using System.Collections;

public class Login : MonoBehaviour 
{
    public UIInput m_InputUserName;
    public UIInput m_InputPassword;

    public GameObject m_ButtonLoginGO;
	// Use this for initialization
	void Start () {
        UIEventListener.Get(m_ButtonLoginGO).onClick = _OnLoginClick;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void _OnLoginClick(GameObject go)
    {

    }
}
