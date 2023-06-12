import { matchInit } from "./handlers/init";
import { matchJoin } from "./handlers/join";
import { matchJoinAttempt } from "./handlers/joinAttempt";
import { matchLeave } from "./handlers/leave";
import { matchLoop } from "./handlers/loop";
import { matchSignal } from "./handlers/signal";
import { matchTerminate } from "./handlers/terminate";
import { moduleName } from "./handlers/common";
import createMatch, { rpcCreateMatch } from "./rpcs/createMatch";

function InitModule(ctx: nkruntime.Context, logger: nkruntime.Logger, nk: nkruntime.Nakama, initializer: nkruntime.Initializer) {
    initializer.registerRpc(createMatch.id, rpcCreateMatch);

    initializer.registerMatch(moduleName, {
        matchInit, matchJoin, matchJoinAttempt, matchLeave, matchLoop, matchSignal, matchTerminate
    });

    logger.info("TypeScript module loaded.");
}

// Reference InitModule to avoid it getting removed on build
!InitModule && InitModule.bind(null);
