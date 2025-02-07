using System.Collections;
using System.Collections.Generic;
using Teo.AutoReference;
using UnityEngine;
using UnityEngine.UI;

public class Test : MonoBehaviour
{
    /*
     * GetComponent
     */
    [SerializeField, Get]
    private Image image;

    /*
     * GetComponentInChildren có tên là Image
     *  CurrentGameObject (Test)
     *      Image   
     */
    [SerializeField, GetInChildren, Name("Image")]
    private Image imageInChildren;

    /*
     * GetComponentInParent có tên là Parent
     * Parent
     *      CurrentGameObject (Test)
     */
    [SerializeField, GetInParent, Name("Parent")]
    private Image imageInParent;

    /*
     * GetComponent của GameObject cùng cha có tên là Sibling
     *  Parent
     *      CurrentGameObject (Test)  
     *      Sibling
     */
    [SerializeField, GetInSiblings, Name("Sibling")]
    private Image imageInSibling;

    /*
     * FindInAssets với đường dẫn 
     */
    [SerializeField, FindInAssets, Path("Assets/AutoReference/Test/TestComponent.prefab")]
    private TestComponent testComponent;

    private void Start()
    {

    }

}
