using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*转为UTF-8*/

namespace MyExtender 
{
    [CreateAssetMenu(fileName = "SkillTree", menuName = "SkillTree", order = 1)]
    public class SkillTree : ScriptableObject
    {
        public List<Skill> skills;
    }

}

