import { cn } from "@/lib/utils";

const sizes = {
  xs: "h-5 w-5 text-[9px]",
  sm: "h-6 w-6 text-[10px]",
  md: "h-8 w-8 text-xs",
  lg: "h-11 w-11 text-sm",
};

// A deterministic colour per identity keeps avatars legible without photos.
const palette = [
  "#6366f1",
  "#ec4899",
  "#14b8a6",
  "#f59e0b",
  "#8b5cf6",
  "#06b6d4",
  "#ef4444",
  "#10b981",
];

function colourFor(seed: string): string {
  let hash = 0;
  for (let i = 0; i < seed.length; i++) hash = (hash * 31 + seed.charCodeAt(i)) >>> 0;
  return palette[hash % palette.length];
}

function initials(name: string): string {
  const parts = name.trim().split(/\s+/);
  return (parts[0]?.[0] ?? "?")
    .concat(parts.length > 1 ? (parts[parts.length - 1][0] ?? "") : "")
    .toUpperCase();
}

/// A coloured initials avatar keyed by a stable seed (user id) so the colour is consistent.
export function UserAvatar({
  name,
  seed,
  size = "md",
  className,
}: {
  name: string;
  seed?: string;
  size?: keyof typeof sizes;
  className?: string;
}) {
  return (
    <span
      className={cn(
        "inline-flex select-none items-center justify-center rounded-full font-semibold text-white ring-2 ring-background",
        sizes[size],
        className,
      )}
      style={{ backgroundColor: colourFor(seed ?? name) }}
      title={name}
    >
      {initials(name)}
    </span>
  );
}
