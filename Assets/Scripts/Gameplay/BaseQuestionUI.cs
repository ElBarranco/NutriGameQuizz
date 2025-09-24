using System;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using UnityEngine.UI;

public class BaseQuestionUI : MonoBehaviour
{
    protected QuestionData question;

    [SerializeField, ReadOnly] protected int guess = 0;

    public int GetAnswer()
    {
        return guess;
    }

    public virtual void Close()
    {
        Destroy(gameObject);
    }

}