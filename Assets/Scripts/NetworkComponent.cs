using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class NetworkComponent : MonoBehaviour
{
	private SocketLib socket;
	private MessageQueue messageQueue;

	public Transform scrollContents;
	public GameObject messagePrefab;

	public InputField msgInput;

	private void Start()
	{
		socket = new SocketLib();
		messageQueue = new MessageQueue();

		socket.AddMessageRecieve(msg =>
		{
			messageQueue.Push(msg);
		});
	}

	public void StartServer()
	{
		messageQueue.Clear();
		socket.StartWithServer();
		StartCoroutine(checkQueue());
	}

	public void StartClient()
	{
		messageQueue.Clear();
		socket.StartWithClient();
		StartCoroutine(checkQueue());
	}

	public void SendMessage()
	{
		string str = msgInput.text;
		msgInput.text = string.Empty;
		socket.SendMessage(22, str);
	}

	/*private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Space))
			messageQueue.Push(new Message() { head = 0, data = "test" });
	}*/

	private IEnumerator checkQueue()
	{
		while (socket.IsClient || socket.IsServer)
		{
			if (messageQueue.Count > 0)
			{
				// get message from queue
				Message msg = messageQueue.Pop();

				// create text object from prefab & setup text object
				GameObject gobj = Instantiate(messagePrefab);
				gobj.GetComponent<Text>().text = msg.data;
				gobj.transform.SetParent(scrollContents, false);

				// setup text object position
				var rectTransform = gobj.GetComponent<RectTransform>();
				var pos = rectTransform.localPosition;
				rectTransform.localPosition = new Vector3(pos.x, pos.y - rectTransform.sizeDelta.y * (scrollContents.childCount - 1), pos.z);
			}

			yield return null;
		}
	}
}
