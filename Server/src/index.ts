import handlers, { moduleName } from "./handlers";
import rpcCreateMatch from "./rpcs/createMatch";

function InitModule(ctx: nkruntime.Context, logger: nkruntime.Logger, nk: nkruntime.Nakama, initializer: nkruntime.Initializer) {
    initializer.registerRpc(rpcCreateMatch.id, rpcCreateMatch.rpc);

    initializer.registerMatch(moduleName, handlers);

    logger.info("TypeScript module loaded.");
}

// Reference InitModule to avoid it getting removed on build
!InitModule && InitModule.bind(null);
