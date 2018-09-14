using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Message
{
	public int head;
	public string data;
}

public class MessageQueue
{
	private Queue<Message> messages;
	public int Count { get { return messages.Count; } }

	public MessageQueue()
	{
		messages = new Queue<Message>();
	}

	public Message Pop() =>
		messages.Count > 0 ?
		messages.Dequeue() :
		new Message() { head = 0, data = string.Empty };

	public void Push(Message msg) => messages.Enqueue(msg);

	public void Clear() => messages.Clear();
}
