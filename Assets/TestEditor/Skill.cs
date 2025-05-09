// 技能类定义 - 增加了children列表支持嵌套
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;


[CreateAssetMenu(fileName = "Skill", menuName = "Skill", order = 1)]
public class Skill : ScriptableObject
{
    [SerializeField]
    public enum SkillType
    {
        Fish,
        Lol,
        Dog
    }

    public SkillType type;

    public int cost;

    public float duration;


    public async Task DoSkill() 
    {
        switch (type)
        {
            case SkillType.Fish:
                 await DoFishTask();
                break;
            case SkillType.Lol:
                await DoLolTask();
                break;
            case SkillType.Dog:
                await DoDogTask();
                break;
            default:
                break;
        }
    }


    private async Task DoFishTask() 
    {
        await Task.Delay((int)(duration * 1000)).ConfigureAwait(false);
        Debug.Log("cost:"+cost);
    }
    private async Task DoLolTask()
    {
        await Task.Delay((int)(duration * 1000)).ConfigureAwait(false);
        Debug.Log("cost:" + cost);
    }
    private async Task DoDogTask()
    {
        await Task.Delay((int)(duration * 1000)).ConfigureAwait(false);
        Debug.Log("cost:" + cost);
    }

}