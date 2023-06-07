import {
    moduleName,
    matchInit,
    matchJoinAttempt,
    matchJoin,
    matchLeave,
    matchLoop,
    matchTerminate,
    matchSignal,
} from "./match_handler";

import { rpcCreateMatch } from "./match_rpc";

function InitModule(ctx: nkruntime.Context, logger: nkruntime.Logger, nk: nkruntime.Nakama, initializer: nkruntime.Initializer) {
    initializer.registerRpc("create_match", rpcCreateMatch);

    initializer.registerMatch(moduleName, {
        matchInit,
        matchJoinAttempt,
        matchJoin,
        matchLeave,
        matchLoop,
        matchTerminate,
        matchSignal,
    });

    logger.info("TypeScript module loaded.");
}

// Reference InitModule to avoid it getting removed on build
!InitModule && InitModule.bind(null);
