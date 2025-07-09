using UnityEngine;

public class Simulated : MonoBehaviour, IEnemyData
{
    public int[] marriagesSuits;
    public int[] alone10;
    int[] IEnemyData.alone10 => alone10;
    int[] IEnemyData.marriagesSuits => marriagesSuits;
}
