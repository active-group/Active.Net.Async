namespace Active.Net

module Async =
    open System.Threading

    let asIfAsynchronously (f: unit -> 'A): Async<'A> =
        async {
            let original = System.Threading.SynchronizationContext.Current
            do! Async.SwitchToNewThread()
            try
                let v = f ()
                do! Async.SwitchToContext(original)
                return v
            with
            | e ->
                do! Async.SwitchToContext(original)
                return raise e
        }

    /// invoke a callback after a bunch of milliseconds, return a function that cancels the timer
    let timer (milliseconds: int) (callback: Async<unit>): unit -> unit =
        let cts = new CancellationTokenSource()
        let token = cts.Token
        let a = async {
                    do! Async.Sleep milliseconds
                    do! callback
                }
        Async.Start (a, token)
        cts.Cancel

