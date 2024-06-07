using NaughtyAttributes;
using System;
using UnityEngine;

public class SetCharacterDatas : MonoBehaviour
{
    [SerializeField] private CharacterData data;

    [Button]
    private void SetData ()
    {
        var objects = GetComponents(typeof(ICharacterDataHolder));
        var span = objects.AsSpan();

        for ( int i = 0; i < span.Length; ++i )
        {
            var obj = (ICharacterDataHolder)span[i];
            obj.SetCharacterData(data);
        }
    }
}

public interface ICharacterDataHolder
{
    public void SetCharacterData ( CharacterData characterData );
}