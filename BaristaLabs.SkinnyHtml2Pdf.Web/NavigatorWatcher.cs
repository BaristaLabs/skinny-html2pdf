namespace BaristaLabs.SkinnyHtml2Pdf.Web
{
    using BaristaLabs.ChromeDevTools.Runtime;
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents an object that watches for page navigation lifecycle events.
    /// </summary>
    public class NavigatorWatcher : IDisposable
    {
        private object m_syncRoot = new object();

        private SemaphoreSlim m_loadSemaphore = null;
        private SemaphoreSlim m_domContentLoadedSemaphore = null;

        private SemaphoreSlim m_networkIdleSemaphore = null;
        private SemaphoreSlim m_networkAlmostIdleSemaphore = null;

        private Timer m_networkActivityTimer = null;
        private volatile int m_networkActivityCount = 0;

        public NavigatorWatcher(ChromeSession session)
        {
            Session = session ?? throw new ArgumentNullException(nameof(session));

            session.Page.SubscribeToLoadEventFiredEvent((e) =>
            {
                if (m_loadSemaphore != null)
                {
                    lock(m_syncRoot)
                    {
                        if (m_loadSemaphore != null)
                            m_loadSemaphore.Release();
                    }
                }
            });

            session.Page.SubscribeToDomContentEventFiredEvent((e) =>
            {
                if (m_domContentLoadedSemaphore != null)
                {
                    lock (m_syncRoot)
                    {
                        if (m_domContentLoadedSemaphore != null)
                            m_domContentLoadedSemaphore.Release();
                    }
                }
                    
            });

            session.Network.SubscribeToRequestWillBeSentEvent((e) =>
            {
                m_networkActivityCount++;
            });

            session.Network.SubscribeToResponseReceivedEvent((e) =>
            {
                m_networkActivityCount++;
            });

            session.Network.SubscribeToDataReceivedEvent((e) =>
            {
                m_networkActivityCount++;
            });
        }

        /// <summary>
        /// Gets the session associated with the watcher
        /// </summary>
        public ChromeSession Session
        {
            get;
        }

        /// <summary>
        /// Begin watching for page lifecycle events.
        /// </summary>
        /// <returns></returns>
        public async Task Start()
        {
            if (m_isDisposed)
                throw new ObjectDisposedException("Navigator Watcher has been disposed.");

            //Enable events.
            await Session.Page.Enable();
            await Session.Network.Enable(new ChromeDevTools.Runtime.Network.EnableCommand()
            {
            });

            m_networkActivityTimer = new Timer((state) =>
            {
                if (m_networkActivityCount <= 2 && m_networkAlmostIdleSemaphore != null)
                {
                    lock (m_syncRoot)
                    {
                        if (m_networkAlmostIdleSemaphore != null)
                            m_networkAlmostIdleSemaphore.Release();
                    }
                }


                if (m_networkActivityCount <= 0 && m_networkIdleSemaphore != null)
                {
                    lock (m_syncRoot)
                    {
                        if (m_networkIdleSemaphore != null)
                            m_networkIdleSemaphore.Release();
                    }
                }

                m_networkActivityCount = 0;
            }, null, 500, 500);
        }

        /// <summary>
        /// Returns a task that resolves when the page load event has fired.
        /// </summary>
        /// <returns></returns>
        public async Task WaitForLoadEvent()
        {
            if (m_isDisposed)
                throw new ObjectDisposedException("Navigator Watcher has been disposed.");

            if (m_loadSemaphore != null)
                throw new InvalidOperationException("Already waiting on load.");

            try
            {
                m_loadSemaphore = new SemaphoreSlim(0, 1);
                await m_loadSemaphore.WaitAsync();
            }
            finally
            {
                m_loadSemaphore.Dispose();
                m_loadSemaphore = null;
            }
        }

        /// <summary>
        /// Returns a task that resolves when the dom content loaded event has been fired.
        /// </summary>
        /// <returns></returns>
        public async Task WaitForDomContentLoaded()
        {
            if (m_isDisposed)
                throw new ObjectDisposedException("Navigator Watcher has been disposed.");

            if (m_domContentLoadedSemaphore != null)
                throw new InvalidOperationException("Already waiting on dom content loaded.");

            try
            {
                m_domContentLoadedSemaphore = new SemaphoreSlim(0, 1);
                await m_domContentLoadedSemaphore.WaitAsync();
            }
            finally
            {
                m_domContentLoadedSemaphore.Dispose();
                m_domContentLoadedSemaphore = null;
            }
        }

        // <summary>
        /// Returns a task that resolves when there are 0 network requests within 500ms.
        /// </summary>
        /// <returns></returns>
        public async Task WaitForNetworkIdle()
        {
            if (m_isDisposed)
                throw new ObjectDisposedException("Navigator Watcher has been disposed.");

            if (m_networkIdleSemaphore != null)
                throw new InvalidOperationException("Already waiting on network idle.");

            try
            {
                m_networkIdleSemaphore = new SemaphoreSlim(0, 1);
                await m_networkIdleSemaphore.WaitAsync();
            }
            finally
            {
                m_networkIdleSemaphore.Dispose();
                m_networkIdleSemaphore = null;
            }
        }

        /// <summary>
        /// Returns a task that resolves until there are 2 or less network requests within 500ms.
        /// </summary>
        /// <returns></returns>
        public async Task WaitForNetworkAlmostIdle()
        {
            if (m_isDisposed)
                throw new ObjectDisposedException("Navigator Watcher has been disposed.");

            if (m_networkIdleSemaphore != null)
                throw new InvalidOperationException("Already waiting on network almost idle.");

            try
            {
                m_networkAlmostIdleSemaphore = new SemaphoreSlim(0, 1);
                await m_networkAlmostIdleSemaphore.WaitAsync();
            }
            finally
            {
                m_networkAlmostIdleSemaphore.Dispose();
                m_networkAlmostIdleSemaphore = null;
            }
        }

        #region IDisposable Support
        private bool m_isDisposed = false;

        private void Dispose(bool disposing)
        {
            if (!m_isDisposed)
            {
                if (disposing)
                {
                    lock (m_syncRoot)
                    {
                        if (m_networkActivityTimer != null)
                        {
                            m_networkActivityTimer.Dispose();
                            m_networkActivityTimer = null;
                        }

                        if (m_loadSemaphore != null)
                        {
                            m_loadSemaphore.Dispose();
                            m_loadSemaphore = null;
                        }

                        if (m_domContentLoadedSemaphore != null)
                        {
                            m_domContentLoadedSemaphore.Dispose();
                            m_domContentLoadedSemaphore = null;
                        }

                        if (m_networkIdleSemaphore != null)
                        {
                            m_networkIdleSemaphore.Dispose();
                            m_networkIdleSemaphore = null;
                        }

                        if (m_networkAlmostIdleSemaphore != null)
                        {
                            m_networkAlmostIdleSemaphore.Dispose();
                            m_networkAlmostIdleSemaphore = null;
                        }
                    }
                }

                m_isDisposed = true;
            }
        }

        /// <summary>
        /// Disposes of the watcher.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion


    }
}
