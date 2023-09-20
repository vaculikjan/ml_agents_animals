// Author: Jan Vaculik

using UnityEngine;

public class Food : MonoBehaviour
{
    [SerializeField]
    private float _FoodValue = 10;
    
    [SerializeField]
    private int _FoodCount;
    
    [SerializeField]
    private float _TimeToEat;
    public float TimeToEat => _TimeToEat;
    
    
    public void Eat(out float foodValue) { 
        foodValue = _FoodValue;
        
        _FoodCount--;
        if (_FoodCount <= 0)
        {
            Destroy(gameObject);
        }
    }
}