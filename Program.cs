using System;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Rug.Osc;
using VRC.OSCQuery;

namespace EyeHeightPersistence
{
	public class Config
	{
		public string IP { get; set; } = "127.0.0.1";
		public int ListeningPort { get; set; } = 9001;
		public int SendingPort { get; set; } = 9000;
		public bool OSCQuery { get; set; } = true;
		public int ChangeDelayMS { get; set; } = 200;
		public bool RelativeMode { get; set; } = true;
		public float HeightTolerance { get; set; } = 0.2f;
	}

	class Program
	{
		static Config config;
		static OscReceiver receiver;
		static OscSender sender;
		static OSCQueryService oscQuery;

		static volatile bool isPaused = false;
		static volatile bool isChanging = false;

		static string lastAvatarId = "";
		static float lastEyeHeight = 1f;
		static float lastScaleFactorInverse = 1f;

		static float currentEyeHeight = 1f;
		static float currentScaleFactorInverse = 1f;

		static async Task Main(string[] args)
		{
			Console.WriteLine("Eye Height Persistence Starting");
			Console.WriteLine("Version: {VERSION_PLACEHOLDER}\n");

			LoadConfig();

			if (config.OSCQuery)
			{
				Console.WriteLine("Using OSCQuery");

				int tcpPort = Extensions.GetAvailableTcpPort();
				int udpPort = Extensions.GetAvailableUdpPort();

				oscQuery = new OSCQueryServiceBuilder()
					.WithServiceName("Eye Height Persistence")
					.WithUdpPort(udpPort)
					.WithTcpPort(tcpPort)
					.WithDefaults()
					.Build();

				oscQuery.AddEndpoint<float>("/avatar/eyeheight", Attributes.AccessValues.ReadWrite);
				oscQuery.AddEndpoint<bool>("/avatar/eyeheightscalingallowed", Attributes.AccessValues.ReadOnly);
				oscQuery.AddEndpoint<float>("/avatar/parameters/ScaleFactorInverse", Attributes.AccessValues.ReadOnly);
				oscQuery.AddEndpoint<string>("/avatar/change", Attributes.AccessValues.ReadOnly);

				receiver = new OscReceiver(IPAddress.Parse(config.IP), udpPort);
				receiver.Connect();

				InitializeSender(config.SendingPort);

				oscQuery.OnOscServiceAdded += (profile) =>
				{
					if (profile.name.Contains("VRChat"))
					{
						InitializeSender(profile.port);
					}
				};
			}
			else
			{
				receiver = new OscReceiver(IPAddress.Parse(config.IP), config.ListeningPort);
				receiver.Connect();
				Console.WriteLine($"Listening on {config.ListeningPort}");

				InitializeSender(config.SendingPort);
			}

			Task listenTask = new Task(ListenLoop);
			listenTask.Start();

			Console.WriteLine("\n\n");

			await Task.Delay(-1);
		}

		static void InitializeSender(int port)
		{
			if (sender != null)
			{
				sender.Dispose();
			}
			sender = new OscSender(IPAddress.Parse(config.IP), 0, port);
			sender.Connect();
			Console.WriteLine($"Sending to port {port}");
		}

		static void LoadConfig()
		{
			if (File.Exists("config.json"))
			{
				string json = File.ReadAllText("config.json");
				config = JsonSerializer.Deserialize<Config>(json);
			}
			else
			{
				config = new Config();
				File.WriteAllText("config.json", JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true }));
			}
		}

		static void ListenLoop()
		{
			while (receiver.State != OscSocketState.Closed)
			{
				if (receiver.State == OscSocketState.Connected)
				{
					OscPacket packet = receiver.Receive();
					if (packet is OscMessage message)
					{
						HandleMessage(message);
					}
				}
			}
		}

		static void HandleMessage(OscMessage msg)
		{
			try
			{
				switch (msg.Address)
				{
					case "/avatar/eyeheightscalingallowed":
						bool _isPaused = !(bool)msg[0];
						if (_isPaused != isPaused)
						{
							isPaused = _isPaused;
							Console.WriteLine($"World scaling allowance changed: {!isPaused}");
						}
						break;

					case "/avatar/eyeheight":
						currentEyeHeight = (float)msg[0];
						if (!isChanging) lastEyeHeight = currentEyeHeight;
						break;

					case "/avatar/parameters/ScaleFactorInverse":
						currentScaleFactorInverse = (float)msg[0];
						if (!isChanging) lastScaleFactorInverse = currentScaleFactorInverse;
						break;

					case "/avatar/change":
						string newId = msg[0].ToString();
						if (!isChanging)
						{
							_ = ProcessAvatarChange(newId);
						}
						break;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error parsing {msg.Address}: {ex.Message}");
			}
		}

		static async Task ProcessAvatarChange(string newId)
		{
			isChanging = true;
			Console.WriteLine($"Avatar Changed");

			await Task.Delay(config.ChangeDelayMS);

			if (!isPaused)
			{
				if (!config.RelativeMode)
				{
					ApplyEyeHeight(lastEyeHeight);
				}
				else if (newId == lastAvatarId)
				{
					ApplyEyeHeight(lastEyeHeight);
				}
				else
				{
					float prevBaseHeight = lastEyeHeight * lastScaleFactorInverse;
					float newBaseHeight = currentEyeHeight * currentScaleFactorInverse;

					Console.WriteLine($"Previous Base Height: {prevBaseHeight:F3}, New Base Height: {newBaseHeight:F3}");

					if (Math.Abs(prevBaseHeight - newBaseHeight) <= config.HeightTolerance)
					{
						float targetEyeHeight = newBaseHeight / lastScaleFactorInverse;
						ApplyEyeHeight(targetEyeHeight);
					}
					else
					{
						ApplyEyeHeight(lastEyeHeight);
					}
				}
			}

			lastAvatarId = newId;
			isChanging = false;
		}

		static void ApplyEyeHeight(float targetHeight)
		{
			Console.WriteLine($"EyeHeight set to {targetHeight:F3}");
			sender.Send(new OscMessage("/avatar/eyeheight", targetHeight));
			lastEyeHeight = targetHeight;
		}
	}
}