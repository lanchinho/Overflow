import { Button } from "@heroui/button";
import { ArrowDownCircleIcon, ArrowUpCircleIcon, CheckCircleIcon as CheckSolid } from "@heroicons/react/24/solid";
import { CheckCircleIcon as CheckOutLined} from "@heroicons/react/24/outline";
import { Answer, Question } from "@/lib/types";

type Props ={
  target: Question | Answer
  currentUserId?: string;
}

const isTargetAnswwer = (target: Question | Answer): target is Answer =>{
  return "questionId" in target;
};

export default function VotingButtons({target, currentUserId}: Props) {
  const isAnswer = isTargetAnswwer(target);

  return (
    <div className="flex-shrink-0 flex flex-col gap-3 items-center justify-start mt-4">
      <Button
        isIconOnly
        variant='light'
      >
        <ArrowUpCircleIcon className="w-12"/>
      </Button>

      <span className="text-xl font-semibold">0</span>

      <Button
        isIconOnly
        variant='light'
      >
        <ArrowDownCircleIcon className="w-12"/>        
      </Button>      
      {isAnswer && (
        <Button
          isIconOnly
          variant='light'
          className="disabled:opacity-100"
          isDisabled={target.accepted}          
        >
          {target.accepted ? (
            <CheckSolid className="text-success"/>
          ) : (
            <CheckOutLined className="size-12 text-default-500"/>          
          )}
          
        </Button>
      )}
    </div>
  );
}
