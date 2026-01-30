using System;
using System.Collections.Generic;
using GGJ.Utility.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GGJ.Gameplay.Messages
{
    public class MessageManager : MonoBehaviour
    {
        [SerializeField] private TMP_Text text;
        [SerializeField] private Image timerBar;
        [SerializeField] private GameObject parent;

        private static Queue<Message> _messageQueue = new();

        private static Message CurrentMessage => _messageQueue.Count > 0 ? _messageQueue.Peek() : null;

        private Message _cachedMessage;

        private RectTransform _rect;
        
        private string _cachedText;
        private bool _disabled;

        private bool Disabled
        {
            get => _disabled;
            set
            {
                if (_disabled == value)
                    return;

                if (value)
                    DeactivateMessageWindow();
                else
                    ActivateMessageWindow();
                _disabled = value;
            }
        }

        private void ActivateMessageWindow()
        {
            if (!_rect)
                return;

            SetParent(true);
        }
        
        private void DeactivateMessageWindow()
        {
            if (!_rect)
                return;
        }

        public static void AddMessage(Message message)
        {
            _messageQueue.Enqueue(message);
        }
        

        private void OnEnable()
        {
            OnDeviceChanged();
            SetMessageUI(null);
            SetParent(false);

            if (parent)
                _rect = (RectTransform) parent.transform;
        }
        
        private void OnDisable()
        {
            _messageQueue.Clear();
        }

        private void Update()
        {
            Tick(Time.deltaTime);
        }

        private void SetMessageUI(Message message)
        {
            if (message == null)
            {
                SetTimer(0);
                SetText(String.Empty);
                Disabled = true;
            }
            else
            {
                SetText(message.Text);
                SetTimer(message.Timer01);
                SetParent(true);
                Disabled = false;
            }
        }

        private void Tick(float dt)
        {
            if (CurrentMessage == null)
            { 
                if (!Disabled)
                    SetMessageUI(null);
                return;
            }

            if (_cachedMessage != CurrentMessage)
            {
                SetMessageUI(CurrentMessage);
                _cachedMessage = CurrentMessage;
            }
            
            CurrentMessage.Tick(dt);
            SetTimer(CurrentMessage.Timer01);
            if (CurrentMessage.Skip)
                Dequeue();
        }

        private void OnDeviceChanged()
        {
            if (!string.IsNullOrEmpty(_cachedText))
                text.text = _cachedText;
        }

        private void SetText(string messageText)
        {
            _cachedText = messageText;
            if (text)
                text.text = _cachedText;
        }
        
        private void SetTimer(float value01)
        {
            if (timerBar)
                timerBar.fillAmount = value01;
        }

        private void SetParent(bool active)
        {
            if (parent)
                parent.SetActive(active);
        }

        private void Dequeue()
        {
            CurrentMessage?.Finish();
            
            if (_messageQueue.Count == 0)
                return;

            _messageQueue.Dequeue();
            SetMessageUI(CurrentMessage);
        }
    }

    public class Message
    {
        public readonly string Text;

        public float Timer;
        
        private readonly float _maxTime;

        private readonly List<Component> _targets;
        
        public float Timer01 => _maxTime > 0 ? Timer / _maxTime : 0;

        public bool Skip => Timer01 == 0;
        

        public Message(string text, float time = 10, List<Component> targets = null)
        {
            Text = text;
            Timer = time;
            _maxTime = time;
            _targets = targets;
        }

        public void Tick(float dt)
        {
            Timer = Mathf.MoveTowards(Timer, 0, dt);
        }

        public void Finish()
        {
            _targets.TryTrigger();
        }
    }
}
