import { Tag } from "@/lib/types";
import { Card, CardBody, CardFooter, CardHeader } from "@heroui/card";
import { Chip } from "@heroui/chip";
import Link from "next/link";

type Props ={
    tag: Tag;
}

export default function TagCard({tag} : Props) {
  return (
    <Card as={Link} href={`/questions?tag=${tag.slug}`} isHoverable isPressable>
      <CardHeader>
        <Chip variant="bordered">{tag.name}</Chip>
      </CardHeader>
      <CardBody>
        <p className="line-clamp-3">{tag.description}</p>
      </CardBody>
      <CardFooter>
        42 questions
      </CardFooter>
    </Card>
  );
}
