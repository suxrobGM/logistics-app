import { definePreset } from "@primeuix/themes";
import Nora from "@primeuix/themes/nora";

/**
 * TMS Portal Custom Theme Preset
 * Light theme (default) + Dark theme override (.dark-theme class)
 *
 * Primary color: Cyan
 * @see https://primeng.org/theming/styled
 */
export const TmsPreset = definePreset(Nora, {
  components: {
    datatable: {
      css: () => `
        .p-datatable .p-datatable-thead > tr > th {
          text-transform: uppercase;
          font-size: 0.75rem;
          letter-spacing: 0.05em;
        }
      `,
    },
    tag: {
      root: {
        fontSize: "0.75rem",
        fontWeight: "500",
        padding: "0.25rem 0.625rem",
      },
    },
    button: {
      css: ({ dt }) => `
        .p-button:not(.p-button-text):not(.p-button-outlined):not(.p-button-link):not([severity]) {
          background: linear-gradient(135deg, ${dt("primary.500")}, ${dt("primary.600")});
          border: none;
          font-weight: 600;
        }
        .p-button:not(.p-button-text):not(.p-button-outlined):not(.p-button-link):not([severity]):hover:not(:disabled) {
          background: linear-gradient(135deg, ${dt("primary.400")}, ${dt("primary.500")});
        }
      `,
    },
  },

  semantic: {
    primary: {
      50: "{cyan.50}",
      100: "{cyan.100}",
      200: "{cyan.200}",
      300: "{cyan.300}",
      400: "{cyan.400}",
      500: "{cyan.500}",
      600: "{cyan.600}",
      700: "{cyan.700}",
      800: "{cyan.800}",
      900: "{cyan.900}",
      950: "{cyan.950}",
    },
    focusRing: {
      width: "2px",
      style: "solid",
      color: "{primary.color}",
      offset: "2px",
    },
    colorScheme: {
      light: {
        primary: {
          color: "{cyan.600}",
          contrastColor: "#ffffff",
          hoverColor: "{cyan.500}",
          activeColor: "{cyan.700}",
        },
        surface: {
          0: "#ffffff",
          50: "#f8fafc",
          100: "#f1f5f9",
          200: "#e2e8f0",
          300: "#cbd5e1",
          400: "#94a3b8",
          500: "#64748b",
          600: "#475569",
          700: "#334155",
          800: "#1e293b",
          900: "#0f172a",
          950: "#020617",
        },
        highlight: {
          background: "color-mix(in srgb, {primary.color} 12%, transparent)",
          focusBackground: "color-mix(in srgb, {primary.color} 20%, transparent)",
          color: "{primary.700}",
          focusColor: "{primary.800}",
        },
      },
      dark: {
        primary: {
          color: "{cyan.500}",
          contrastColor: "{surface.950}",
          hoverColor: "{cyan.400}",
          activeColor: "{cyan.300}",
        },
        surface: {
          0: "#0f1419",
          50: "#1a1f2e",
          100: "#242b3d",
          200: "#2d3548",
          300: "#363f54",
          400: "#3d4760",
          500: "#4d5a78",
          600: "#64748b",
          700: "#94a3b8",
          800: "#cbd5e1",
          900: "#e2e8f0",
          950: "#f1f5f9",
        },
        highlight: {
          background: "color-mix(in srgb, {primary.color} 16%, transparent)",
          focusBackground: "color-mix(in srgb, {primary.color} 24%, transparent)",
          color: "{primary.400}",
          focusColor: "{primary.300}",
        },
        text: {
          color: "{surface.950}",
          hoverColor: "#ffffff",
          mutedColor: "{surface.700}",
          hoverMutedColor: "{surface.800}",
        },
        content: {
          background: "{surface.50}",
          hoverBackground: "{surface.100}",
          borderColor: "{surface.200}",
          color: "{surface.950}",
          hoverColor: "#ffffff",
        },
        formField: {
          background: "{surface.50}",
          disabledBackground: "{surface.100}",
          filledBackground: "{surface.100}",
          filledHoverBackground: "{surface.200}",
          filledFocusBackground: "{surface.100}",
          borderColor: "{surface.400}",
          hoverBorderColor: "{surface.500}",
          focusBorderColor: "{primary.color}",
          invalidBorderColor: "{red.500}",
          color: "{surface.950}",
          disabledColor: "{surface.600}",
          placeholderColor: "{surface.600}",
          floatLabelColor: "{surface.700}",
          floatLabelFocusColor: "{primary.color}",
          floatLabelActiveColor: "{surface.700}",
          floatLabelInvalidColor: "{red.500}",
          iconColor: "{surface.700}",
          shadow: "none",
        },
        navigation: {
          item: {
            focusBackground: "{surface.100}",
            activeBackground: "{surface.200}",
            color: "{surface.700}",
            focusColor: "{surface.950}",
            activeColor: "{primary.color}",
            icon: {
              color: "{surface.700}",
              focusColor: "{surface.950}",
              activeColor: "{primary.color}",
            },
          },
          submenuLabel: {
            background: "transparent",
            color: "{surface.600}",
          },
          submenuIcon: {
            color: "{surface.700}",
            focusColor: "{surface.950}",
            activeColor: "{primary.color}",
          },
        },
        list: {
          option: {
            focusBackground: "{surface.100}",
            selectedBackground: "color-mix(in srgb, {primary.color} 16%, transparent)",
            selectedFocusBackground: "color-mix(in srgb, {primary.color} 24%, transparent)",
            color: "{surface.950}",
            focusColor: "{surface.950}",
            selectedColor: "{primary.400}",
            selectedFocusColor: "{primary.300}",
            icon: {
              color: "{surface.700}",
              focusColor: "{surface.950}",
            },
          },
          optionGroup: {
            background: "transparent",
            color: "{surface.600}",
          },
        },
        overlay: {
          select: {
            background: "{surface.50}",
            borderColor: "{surface.200}",
            color: "{surface.950}",
          },
          popover: {
            background: "{surface.50}",
            borderColor: "{surface.200}",
            color: "{surface.950}",
          },
          modal: {
            background: "{surface.50}",
            borderColor: "{surface.200}",
            color: "{surface.950}",
          },
        },
        mask: {
          background: "rgba(0, 0, 0, 0.6)",
          color: "{surface.950}",
        },
      },
    },
  },
});

/**
 * PrimeNG theme configuration
 * Uses .dark-theme class for dark mode (managed by theme-toggle component)
 * @see https://primeng.org/tailwind
 */
export const TmsThemeOptions = {
  darkModeSelector: ".dark-theme",
  cssLayer: {
    name: "primeng",
    order: "theme, base, primeng",
  },
};
