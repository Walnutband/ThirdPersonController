
using UnityEditor;
using UnityEngine;

public class CommandTest : MonoBehaviour
{
    public TestSO testSO;
    public string path;

    [ContextMenu("LoadTestSO")]
    private void LoadTestSO()
    {
        testSO = AssetDatabase.LoadAssetAtPath<TestSO>(path);
    }

    [ContextMenu("AddTestSO")]
    private void AddTestSO()
    {
        testSO.a += 10;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("TriggerEnter" + other.gameObject.name);
    }
}


[CreateAssetMenu(fileName = "CommandTest", menuName = "CommandTest", order = 0)]
public class TestSO : ScriptableObject {
    public int a;
}