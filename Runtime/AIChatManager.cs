using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UNIHper;
using DNHper;
using YKT.AI.TotalApi;
using System;
using UniRx;
using System.Threading;
using System.Threading.Tasks;
using DeepSeek.Sdk;
using System.Text;
using System.Linq;

namespace AIChat
{
    public class AIChatManager
    {
        static IAiService AIService = new AiService();

        private static string appKey => Managements.Config.Get<AIChatConfig>().DecryptedAppKey;

        static AIChatManager()
        {
            // AIService.Init(
            //     new YKTInitPo() { Appkey = appKey, EnumApiChannel = EnumApiChannel.Deepseek }
            // );
            Debug.Log("初始化成功");
        }

        public static IObservable<string> ChatStream(string content, string prompt)
        {
            return Observable.Create<string>(observer =>
            {
                var resultMsg = new StringBuilder();
                // api密钥
                string apikey = appKey;
                // 创建ds对象
                var ds = new DeepSeek.Sdk.DeepSeek(apikey);

                Observable
                    .Start(async () =>
                    {
                        Debug.Log(Thread.CurrentThread.ManagedThreadId);
                        // 模型
                        var chatReq = new ChatRequest
                        {
                            Messages = new List<ChatRequest.MessagesType>
                            {
                                new ChatRequest.MessagesType
                                {
                                    Role = ChatRequest.RoleEnum.System,
                                    Content = prompt
                                },
                                // new ChatRequest.MessagesType
                                // {
                                //     Role = ChatRequest.RoleEnum.Assistant,
                                //     Content = resultMsg.ToString()
                                // },
                                new ChatRequest.MessagesType
                                {
                                    Role = ChatRequest.RoleEnum.User,
                                    Content = content
                                }
                            },
                            Model = ChatRequest.ModelEnum.DeepseekChat,
                            Stream = true
                        };
                        if (string.IsNullOrEmpty(prompt))
                        {
                            chatReq.Messages.RemoveAt(0);
                        }

                        resultMsg.Clear(); // 拼接完成则清空, 进行下一轮拼接

                        await ds.ChatStream(
                            chatReq,
                            openedCallBack: (state) => // 打开状态
                            {
                                Console.WriteLine(state);
                            },
                            closedCallBack: (state) => // 关闭状态
                            {
                                Console.WriteLine(state);
                            },
                            msgCallback: (res) => // 接收信息
                            {
                                string msg = res.Choices.FirstOrDefault()?.Delta?.Content;
                                resultMsg.Append(msg);
                                observer.OnNext(res.Choices.FirstOrDefault()?.Delta?.Content);
                            },
                            errorCallback: (ex) => // 异常处理
                            {
                                Console.WriteLine(ex);
                            }
                        );

                        observer.OnCompleted();
                        await Task.CompletedTask;
                    })
                    .Subscribe();

                return Disposable.Empty;
            });
        }

        public static IObservable<string> Chat(string content, string prompt)
        {
            return Observable.Create<string>(observer =>
            {
                // api密钥
                string apikey = appKey;
                // 创建ds对象
                var ds = new DeepSeek.Sdk.DeepSeek(apikey);
                var resultMsg = new StringBuilder();
                Task.Run(async () =>
                {
                    // 模型查询
                    // var modelList = await ds.GetModelList();
                    // 余额查询
                    // var resBalance = await ds.GetBalance();

                    var chatReq = new ChatRequest
                    {
                        Messages = new List<ChatRequest.MessagesType>
                        {
                            new ChatRequest.MessagesType
                            {
                                Role = ChatRequest.RoleEnum.System,
                                Content = prompt
                            },
                            new ChatRequest.MessagesType
                            {
                                Role = ChatRequest.RoleEnum.User,
                                Content = content
                            }
                        },
                        Model = ChatRequest.ModelEnum.DeepseekChat
                    };
                    var chatRes = await ds.Chat(chatReq);
                    chatRes.Choices.ForEach(_choice =>
                    {
                        observer.OnNext(_choice.Message.Content);
                    });
                    observer.OnCompleted();
                });

                return Disposable.Empty;
            });
        }

        // TODO 其他模型
        private static IObservable<string> Chat1(string content, string prompt)
        {
            return Observable.Create<string>(observer =>
            {
                CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

                Observable
                    .Start(async () =>
                    {
                        Debug.Log("开始");
                        await foreach (
                            var res in AIService.ChatYeildContentAsync(
                                new YKTResquestPo()
                                {
                                    Title = content,
                                    enumApiChannel = EnumApiChannel.Deepseek,
                                    Model = "deepseek-chat",
                                    IsStream = true
                                }
                            )
                        )
                        {
                            if (cancellationTokenSource.IsCancellationRequested)
                            {
                                observer.OnError(new Exception("已取消"));
                                break;
                            }
                            observer.OnNext(res);
                        }
                        observer.OnCompleted();
                    })
                    .Subscribe();
                return cancellationTokenSource;
            });
        }
    }
}
